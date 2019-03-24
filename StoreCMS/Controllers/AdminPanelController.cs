using Trane.Db.Context;
using Trane.Db.TypesForEntities;
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
            var user = DataChecker.CheckCookiesForLF(db, HttpContext);
            if (user == null || 
                user.UserType.SecurityClearance == SecurityClearance.Without)
                return View();
            return AdminPanel(user.UserType.SecurityClearance);
        }

        [HttpPost]
        public IActionResult LoginForm(LoginFormData data)
        {
            var user = DataChecker.CheckLoginFormData(db, data);
            if (user == null ||
                user.UserType.SecurityClearance == SecurityClearance.Without)
                return View(data);
            ActionsWithDb.AddConnectedUser(db, user, HttpContext);
            return AdminPanel(user.UserType.SecurityClearance);
        }

        [NonAction]
        public IActionResult AdminPanel(SecurityClearance securityClearance)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var cu in db.ConnectedUsers)
            {
                builder.Append($"userName: {cu.UserName}\n");
                builder.Append($"loginKey: {cu.LoginKey}\n");
                builder.Append($"loginTime: {cu.LastActionTime}\n");
                builder.Append($"User-Agent: {cu.UserAgent}\n");
                db.Entry(cu).Reference(_cu => _cu.User).Load();
                db.Entry(cu.User).Reference(u => u.UserType).Load();
                builder.Append($"User: {cu.User.ID}-{cu.User.Login}\n");
                builder.Append($"Password: {cu.User.Password}\n");
                builder.Append($"Type: {cu.User.UserType.ID}-{cu.User.UserType.Name}\n");
                builder.Append($"{cu.User.UserType.SecurityClearance}\n\n");
            }
            return Content(builder.ToString());
        }
    }
}