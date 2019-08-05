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
        public IActionResult Users()
        {
            HttpContext.Items["pageID"] = AdminPanelPages.Users;
            User[] users = db.Users.ToArray();
            foreach(var user in users)
            {
                db.Entry(user).Reference(u => u.UserType).Load();
                db.Entry(user.UserType).State = EntityState.Detached;
                db.Entry(user).State = EntityState.Detached;
            }
            return View("Users", users);
        }
    }
}