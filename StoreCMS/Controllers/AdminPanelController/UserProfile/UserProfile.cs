using Microsoft.AspNetCore.Mvc;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult UserProfile()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.UserProfile;
            User user = HttpContext.Items["User"] as User;
            db.Entry(user).Collection(u => u.AdminPanelLogs).Load();
            user.AdminPanelLogs.Reverse();
            return View("UserProfile", user.AdminPanelLogs);
        }
    }
}