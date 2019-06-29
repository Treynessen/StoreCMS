using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Security;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [HttpGet]
        public IActionResult AdminPanel(Model model)
        {
            // Проверяем кукисы пользователя на наличие информации о предыдущем входе
            // Если кукисы некорректны, либо вышло время возможного бездействия, то
            // отправляем пользователя на логин форму
            User user = SecurityFunctions.CheckCookies(db, HttpContext);
            if (user == null)
                return LoginForm();
            // Создаем объект для проверки уровней доступа и передаем его в контейнер Items
            // Объект AccessLevelConfiguration используется в методе HasAccessTo(...)
            // Передача в контейнер обязательна, т.к. эти методы вызываются не только внутри контроллера,
            // но и внутри представлений
            AccessLevelConfiguration accessLevelConfiguration = HttpContext.RequestServices.GetService<AccessLevelConfiguration>();
            HttpContext.Items["AccessLevelConfiguration"] = accessLevelConfiguration;
            if (!SecurityFunctions.HasAccessTo(AdminPanelPages.MainPage, user, HttpContext))
                return LoginForm();
            if (!SecurityFunctions.HasAccessTo(model.PageId, user, HttpContext))
                model.PageId = AdminPanelPages.MainPage;

            HttpContext.Items["User"] = user;

            switch (model.PageId)
            {
                case AdminPanelPages.Pages:
                    return Pages();
                case AdminPanelPages.AddPage:
                    return AddPage();
                case AdminPanelPages.EditPage:
                    return EditPage(model.PageType, model.itemID);
                case AdminPanelPages.Categories:
                    return Categories();
                case AdminPanelPages.CategoryProducts:
                    return CategoryProducts(model.itemID);
                case AdminPanelPages.AddProduct:
                    return AddProduct();
                case AdminPanelPages.EditProduct:
                    return EditProduct(model.itemID);
                case AdminPanelPages.ProductImages:
                    return ProductImages(model.itemID);
                case AdminPanelPages.Templates:
                    return Templates();
                case AdminPanelPages.AddTemplate:
                    return AddTemplate();
                case AdminPanelPages.EditTemplate:
                    return EditTemplate(model.itemID);
                default:
                    return MainPage();
            }
        }
    }
}