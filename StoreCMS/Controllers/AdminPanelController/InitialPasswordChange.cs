using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Treynessen.AdminPanelTypes;
using Treynessen.Database;
using Treynessen.Database.Entities;
using Treynessen.Security;

namespace Treynessen.Controllers
{
    [TypeFilter(typeof(SessionExceptionFilter))]
    public partial class AdminPanelController
    {
        private User GetAuthenticatedUser()
        {
            const string key = "StoreCMS.AuthenticatedUserChecked";
            if (!HttpContext.Items.ContainsKey(key))
            {
                HttpContext.Items[key] = true;
                HttpContext.Items["User"] = SecurityFunctions.CheckCookies(db, HttpContext);
            }
            return HttpContext.Items["User"] as User;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpContext.Response.Headers["Cache-Control"] = "no-store";
            if (!HttpContext.Request.IsHttps)
            {
                context.Result = StatusCode(403, "Для административной панели требуется HTTPS.");
                return;
            }
            var user = GetAuthenticatedUser();
            if (user != null && user.RequirePasswordChange)
            {
                if (HttpContext.Request.Method == "GET")
                    context.Result = View("ChangeInitialPassword");
                else
                {
                    context.ActionArguments.TryGetValue("model", out object model);
                    context.ActionArguments.TryGetValue("pageID", out object pageID);
                    bool changing = HttpContext.Request.Method == "POST"
                        && (model as Model)?.PageId == AdminPanelPages.ChangeInitialPassword;
                    bool exiting = HttpContext.Request.Method == "DELETE"
                        && pageID is AdminPanelPages && (AdminPanelPages)pageID == AdminPanelPages.Exit;
                    if (!changing && !exiting)
                        context.Result = StatusCode(403, "Сначала смените первоначальный пароль.");
                }
            }
            base.OnActionExecuting(context);
        }

        [NonAction]
        private IActionResult ChangeInitialPassword(User user, UserModel model)
        {
            DatabaseInteraction.ChangeInitialPassword(db, user, model, out int statusCode, HttpContext);
            if (statusCode == 200)
            {
                SessionCookies.ClearLegacy(HttpContext);
                return LocalRedirect(HttpContext.Request.PathBase.Add(HttpContext.Request.Path).Value);
            }
            HttpContext.Response.StatusCode = statusCode;
            ViewData["PasswordChangeError"] = "Пароль не изменён. Проверьте текущий пароль и повтор нового. Новый пароль должен отличаться от admin и содержать не менее 5 допустимых символов.";
            return View("ChangeInitialPassword");
        }
    }
}
