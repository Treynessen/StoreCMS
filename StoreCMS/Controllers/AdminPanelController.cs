using Trane.Db.Context;
using Trane.ViewModels;
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

        [HttpGet]
        public IActionResult LoginForm()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoginForm(LoginFormData data)
        {
            if (DataChecker.CorrectUserData(db, data))
                return AdminPanel();
            return View(data);
        }

        public IActionResult AdminPanel()
        {
            return Content("Admin panel");
        }
    }
}