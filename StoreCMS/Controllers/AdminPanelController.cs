using Trane.Db.Entities;
using Trane.Db.Context;
using Trane.ViewModels;
using Trane.Controllers.Models;
using Trane.Functions;
using Trane.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace Trane.Controllers
{
    public class AdminPanelController : Controller
    {
        private CMSContext db;

        public AdminPanelController(CMSContext db)
        {
            this.db = db;
        }

        // По умолчанию AdminPanel должен отправлять пользователя либо
        // на LoginForm, либо на MainPage
        [HttpGet]
        public IActionResult AdminPanel(AdminPanelPages pageID)
        {
            SetRoutes("AdminPanel(GET)");
            User user = DataChecker.CheckCookies(db, HttpContext);
            if (!DataChecker.HasAccessTo(pageID, user, HttpContext))
            {
                if (!DataChecker.HasAccessTo(AdminPanelPages.MainPage, user, HttpContext))
                    return LoginForm();
                pageID = AdminPanelPages.MainPage;
            }
            HttpContext.Items["User"] = user;
            switch (pageID)
            {
                case AdminPanelPages.Pages:
                    return PagesGet();
                case AdminPanelPages.AddPage:
                    return AddPage();
                case AdminPanelPages.Settings:
                    return SettingsGet();
                default:
                    return MainPageGet();
            }
        }

        [HttpPost]
        public IActionResult AdminPanel(AdminPanelModel model, LoginFormModel lfModel)
        {
            SetRoutes("AdminPanel(POST)");
            User user = DataChecker.CheckCookies(db, HttpContext);
            if (user == null)
            {
                if (!DataChecker.IsValidLoginFormData(db, lfModel, HttpContext))
                    return LoginForm(lfModel);
                return RedirectToAction(nameof(AdminPanel));
            }
            HttpContext.Items["User"] = user;
            switch (model.PageId)
            {
                case AdminPanelPages.AddPage:
                    if (ActionsWithDb.AddSimplePage(db, model.SimplePage) == false)
                        return AddPage(model.SimplePage);
                    return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Pages}");
                default:
                    return RedirectToAction(nameof(AdminPanel));
            }
        }

        [NonAction]
        public IActionResult LoginForm(LoginFormModel model = null)
        {
            SetRoutes("LoginForm(GET)");
            return View("LoginForm", model);
        }

        [NonAction]
        public IActionResult MainPageGet()
        {
            SetRoutes("MainPage(GET)");
            return View("MainPage");
        }

        [NonAction]
        public IActionResult PagesGet()
        {
            SetRoutes("Pages(GET)");
            return View("Pages/Index");
        }

        [NonAction]
        public IActionResult AddPage(SimplePage page = null)
        {
            SetRoutes("AddPage(GET)");
            return View("Pages/AddPage", page);
        }

        [NonAction]
        public IActionResult EditPage(int id)
        {
            SetRoutes("EditPage(GET)");
            return View("Pages/EditPage");
        }

        [NonAction]
        public IActionResult SettingsGet()
        {
            SetRoutes("Settings(GET)");
            return View("Settings");
        }

        private void SetRoutes(string name)
        {
            if (string.IsNullOrEmpty(HttpContext.Items["Routes"] as string))
                HttpContext.Items["Routes"] = name;
            else HttpContext.Items["Routes"] += $" -> {name}";
        }
    }
}