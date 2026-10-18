using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.AdminPanelTypes;
using Treynessen.Security;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // [Комментарий ИИ-агента]: Сначала сохраняем HTTPS, проверку сессии, селектора и первоначального пароля из существующего фильтра.
            OnActionExecuting(context);
            if (context.Result != null)
                return;

            string method = HttpContext.Request.Method;
            if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method) && !HttpMethods.IsOptions(method))
            {
                context.ActionArguments.TryGetValue("model", out object model);
                context.ActionArguments.TryGetValue("pageID", out object pageID);
                AdminPanelPages? page = HttpMethods.IsPost(method) ? (model as Model)?.PageId
                    : pageID is AdminPanelPages value ? value : (AdminPanelPages?)null;
                bool login = HttpMethods.IsPost(method) && page == AdminPanelPages.LoginForm;
                bool exit = HttpMethods.IsDelete(method) && page == AdminPanelPages.Exit;
                var user = GetAuthenticatedUser();
                if (!login && !exit)
                {
                    if (user == null)
                    {
                        context.Result = StatusCode(401);
                        return;
                    }
                    var access = HttpContext.RequestServices.GetRequiredService<AccessLevelConfiguration>();
                    HttpContext.Items["AccessLevelConfiguration"] = access;
                    if (page != AdminPanelPages.ChangeInitialPassword && !SecurityFunctions.HasAccessTo(page, user, HttpContext))
                    {
                        context.Result = StatusCode(403);
                        return;
                    }
                }

                // [Комментарий ИИ-агента]: Штатная проверка охватывает все изменяющие action, включая вход и идемпотентный выход; отказ не вызывает бизнес-операцию.
                try
                {
                    await HttpContext.RequestServices.GetRequiredService<IAntiforgery>().ValidateRequestAsync(HttpContext);
                }
                catch (AntiforgeryValidationException)
                {
                    context.Result = StatusCode(400);
                    return;
                }
            }

            var executed = await next();
            OnActionExecuted(executed);
            ApplyNativePostRedirect(context, executed);
        }
    }
}
