using Trane.Db.Entities;
using Trane.Db.Context;
using Trane.ViewModels;
using Trane.Controllers.Models;
using Trane.Functions;
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
        public IActionResult AdminPanel()
        {
            User user = DataChecker.CheckCookies(db, HttpContext);
            if (!DataChecker.HasAccessTo(AdminPanelPages.MainPage, user))
            {
                return LoginForm();
            }
            // Если валидация прошла успешно, то отправляем пользователя в админ панель
            return MainPage(user);
        }

        [HttpPost]
        public IActionResult AdminPanel(AdminPanelModel model, LoginFormData lfData)
        {
            if (model.PageId == AdminPanelPages.LoginFormPage)
            {
                // Проверяем введенные данные. Если не проходят валидацию, то
                // отправляем LoginFormData обратно в LoginForm
                if (!DataChecker.IsValidLoginFormData(db, lfData, HttpContext))
                {
                    return LoginForm(lfData);
                }
            }
            else
            {
                User user = DataChecker.CheckCookies(db, HttpContext);
                if (!DataChecker.HasAccessTo(model.PageId, user))
                {
                    if (!DataChecker.HasAccessTo(AdminPanelPages.MainPage, user))
                        return LoginForm(lfData);
                    else
                    {
                        return RedirectToAction(nameof(AdminPanelController.AdminPanel));
                    }
                }

                switch (model.PageId)
                {

                }
            }
            return RedirectToAction(nameof(AdminPanelController.AdminPanel));
        }

        [NonAction]
        public IActionResult LoginForm(LoginFormData data = null)
        {
            return View("LoginForm", data);
        }

        [NonAction]
        public IActionResult MainPage(User user)
        {
            return View("MainPage", user);
        }
    }
}