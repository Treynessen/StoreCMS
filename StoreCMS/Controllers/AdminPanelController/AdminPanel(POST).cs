using Microsoft.AspNetCore.Mvc;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [HttpPost]
        public IActionResult AdminPanel(AdminPanelModel model, LoginFormModel lfModel)
        {
            SetRoutes("AdminPanel(POST)");

            User user = DataCheck.CheckCookies(db, HttpContext);
            if (user == null)
            {
                if (!DataCheck.IsValidLoginFormData(db, lfModel, HttpContext))
                    return LoginForm(lfModel);
                return RedirectToAction(nameof(AdminPanel));
            }
            if (!DataCheck.HasAccessTo(model.PageId.Value, user, HttpContext))
                return RedirectToAction(nameof(AdminPanel));

            HttpContext.Items["User"] = user;

            switch (model.PageId)
            {
                case AdminPanelPages.AddPage:
                    if (ActionsWithDatabase.AddPage(db, model.PageModel, HttpContext) == false)
                        return AddPage(model.PageModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");

                case AdminPanelPages.EditPage:
                    if (ActionsWithDatabase.EditPage(db, model, HttpContext) == false)
                        return EditPage(model.PageType, model.itemID, model.PageModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.EditPage}&pageType={(int)model.PageType}&itemID={model.itemID}");

                case AdminPanelPages.DeletePage:
                    ActionsWithDatabase.DeletePage(db, model.PageType, model.itemID);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");

                case AdminPanelPages.AddTemplate:
                    if (ActionsWithDatabase.AddTemplates(db, model.TemplateModel, HttpContext) == false)
                        return AddTemplate(model.TemplateModel);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Templates}");

                default:
                    return RedirectToAction(nameof(AdminPanel));
            }
        }
    }
}
