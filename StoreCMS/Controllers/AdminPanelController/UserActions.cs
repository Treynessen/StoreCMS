using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult UserActions(int? userID)
        {
            HttpContext.Items["pageID"] = AdminPanelPages.UserActions;
            if (!userID.HasValue)
            {
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Users}");
            }
            User user = db.Users.AsNoTracking().FirstOrDefault(u => u.ID == userID.Value);
            if (user == null)
            {
                return Redirect($"{HttpContext.Request.Path}?pageID={(int)AdminPanelPages.Users}");
            }
            user.AdminPanelLogs = db.AdminPanelLogs.AsNoTracking().Where(al => al.UserId == user.ID).ToList();
            return View("UserActions", user);
        }
    }
}