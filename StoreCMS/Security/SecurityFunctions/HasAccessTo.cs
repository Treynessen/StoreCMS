using System;
using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Security
{
    public static partial class SecurityFunctions
    {
        // Проверка прав пользователя для доступа к страницам админ панели
        public static bool HasAccessTo(AdminPanelPages? page, User user, HttpContext context)
        {
            if (user == null)
                return false;
            if (!page.HasValue)
                return false;
            if (!Enum.IsDefined(typeof(AdminPanelPages), page.Value))
                return false;
            AccessLevelConfiguration config = context.Items["AccessLevelConfiguration"] as AccessLevelConfiguration;
            return user.UserType.AccessLevel >= config.GetAccessLevelInfoTo(page.ToString());
        }
    }
}