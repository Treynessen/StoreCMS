using Microsoft.AspNetCore.Mvc;
using Treynessen.Database.Entities;
using Treynessen.AdminPanelTypes;
using Treynessen.Functions;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [HttpGet]
        public IActionResult AdminPanel(AdminPanelModel model)
        {
            SetRoutes("AdminPanel(GET)");

            User user = DataCheck.CheckCookies(db, HttpContext);
            if(!DataCheck.HasAccessTo(AdminPanelPages.MainPage, user, HttpContext))
                return LoginForm();
            if (!DataCheck.HasAccessTo(model.PageId, user, HttpContext))
                model.PageId = AdminPanelPages.MainPage;

            HttpContext.Items["User"] = user;

            switch (model.PageId.Value)
            {
                case AdminPanelPages.Pages:
                    return Pages();
                case AdminPanelPages.AddPage:
                    return AddPage();
                case AdminPanelPages.EditPage:
                    return EditPage(model.PageType, model.itemID);
                case AdminPanelPages.Templates:
                    return Templates();
                case AdminPanelPages.AddTemplate:
                    return AddTemplate();
                case AdminPanelPages.EditTemplate:
                    return EditTemplate(model.itemID);
                case AdminPanelPages.Settings:
                    return Settings();
                default:
                    return MainPage();
            }
        }
    }
}