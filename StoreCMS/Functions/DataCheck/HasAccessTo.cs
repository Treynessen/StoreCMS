using Microsoft.AspNetCore.Http;
using Trane.OtherTypes;
using Trane.DependencyInjection;
using Trane.Database.Entities;

namespace Trane.Functions
{
    public static partial class DataCheck
    {
        // Проверка прав пользователя для доступа к страницам админ панели
        public static bool HasAccessTo(AdminPanelPages page, User user, HttpContext context)
        {
            if (user == null)
                return false;
            AccessLevelConfiguration config = context.Items["AccessLevelConfiguration"] as AccessLevelConfiguration;
            switch (page)
            {
                case AdminPanelPages.MainPage:
                    return user.UserType.AccessLevel >= config.ShowMainPage;
                case AdminPanelPages.Pages:
                    return user.UserType.AccessLevel >= config.ShowPages;
                case AdminPanelPages.AddPage:
                    return user.UserType.AccessLevel >= config.AddPage;
                case AdminPanelPages.Settings:
                    return user.UserType.AccessLevel >= config.AccessToSettings;
                default:
                    return false;
            }
        }
    }
}
