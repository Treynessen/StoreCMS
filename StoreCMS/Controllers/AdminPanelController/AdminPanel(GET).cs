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
            if (model.PageId.HasValue && model.PageId.Value == AdminPanelPages.UserActions)
            {
                if (!SecurityFunctions.HasAccessTo(AdminPanelPages.GetUserLog, user, HttpContext))
                    model.PageId = AdminPanelPages.MainPage;
            }
            else
            {
                if (!SecurityFunctions.HasAccessTo(model.PageId, user, HttpContext))
                    model.PageId = AdminPanelPages.MainPage;
            }

            HttpContext.Items["User"] = user;

            switch (model.PageId)
            {
                case AdminPanelPages.Pages:
                    return Pages();
                case AdminPanelPages.AddPage:
                    return AddPage();
                case AdminPanelPages.EditPage:
                    return EditPage(model.itemID);
                case AdminPanelPages.Categories:
                    return Categories();
                case AdminPanelPages.AddCategory:
                    return AddCategory();
                case AdminPanelPages.EditCategory:
                    return EditCategory(model.itemID);
                case AdminPanelPages.CategoryProducts:
                    return CategoryProducts(model.itemID);
                case AdminPanelPages.AddProduct:
                    return AddProduct(model.itemID);
                case AdminPanelPages.EditProduct:
                    return EditProduct(model.itemID);
                case AdminPanelPages.ProductImages:
                    return ProductImages(model.itemID);
                case AdminPanelPages.Redirections:
                    return Redirections();
                case AdminPanelPages.Templates:
                    return Templates();
                case AdminPanelPages.AddTemplate:
                    return AddTemplate();
                case AdminPanelPages.EditTemplate:
                    return EditTemplate(model.itemID);
                case AdminPanelPages.Chunks:
                    return Chunks();
                case AdminPanelPages.AddChunk:
                    return AddChunk();
                case AdminPanelPages.EditChunk:
                    return EditChunk(model.itemID);
                case AdminPanelPages.FileManager:
                    return FileManager(model.Path);
                case AdminPanelPages.EditStyle:
                    return EditCssFile(model.Path);
                case AdminPanelPages.EditScript:
                    return EditScriptFile(model.Path);
                case AdminPanelPages.Users:
                    return Users();
                case AdminPanelPages.UserActions:
                    return UserActions(model.itemID);
                case AdminPanelPages.UserTypes:
                    return UserTypes();
                case AdminPanelPages.SynonymsForStrings:
                    return SynonymsForStrings();
                case AdminPanelPages.UserProfile:
                    return UserProfile();
                case AdminPanelPages.Settings:
                    return Settings();
                case AdminPanelPages.GetUserLog:
                    return GetUserLog(model.itemID, model.CurrentLogDate, HttpContext);
                default:
                    return MainPage();
            }
        }
    }
}