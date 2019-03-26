using Trane.Db.Entities;
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
        public IActionResult AdminPanel()
        {
            // При http-запросе .../admin проверяем кукисы пользователя
            User user = DataChecker.CheckCookiesForLF(db, HttpContext);

            // Если user == null (отсутствуют кукисы или вышло время бездействия)
            // или у пользователя низкий уровень доступа,
            // то отправляем его на логин форму. Проверка данных производится в
            // post-методе AdminPanel
            if (user == null || user.UserType.SecurityClearance == SecurityClearance.Without)
                return LoginForm();

            // Если верификация прошла успешно, то отправляем пользователя в админ панель
            return Panel(user);
        }

        [HttpPost]
        public IActionResult AdminPanel(LoginFormData data)
        {
            // Проверяем полученные данные из логин формы
            User user = DataChecker.CheckLoginFormData(db, data);

            // Если user == null или не имеет достаточного уровня допуска,
            // тогда возвращаем его обратно в LoginForm
            if (user == null || user.UserType.SecurityClearance == SecurityClearance.Without)
                return LoginForm(data);

            // Иначе сохраняем пользователя в БД и возвращаемся в get версию метода AdminPanel
            ActionsWithDb.AddConnectedUser(db, user, HttpContext);
            return RedirectToAction(nameof(AdminPanelController.AdminPanel));
        }

        [NonAction]
        public IActionResult LoginForm(LoginFormData data = null)
        {
            return View("LoginForm", data);
        }

        [NonAction]
        public IActionResult Panel(User user)
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