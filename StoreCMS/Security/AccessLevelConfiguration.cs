using System;
using Microsoft.Extensions.Configuration;

namespace Treynessen.Security
{
    public class AccessLevelConfiguration
    {
        private IConfigurationSection accessConfiguration;

        public AccessLevelConfiguration(IConfigurationSection accessConfiguration)
        {
            this.accessConfiguration = accessConfiguration;
        }

        public AccessLevel GetAccessLevelInfoTo(string pageName)
        {
            AccessLevel accessLevel;
            string value = accessConfiguration[pageName];
            // Если настройка не определена в конфигурации, то выставляем ей максимальный уровень доступа
            if (string.IsNullOrEmpty(value))
                return AccessLevel.VeryHigh;
            try
            {
                accessLevel = (AccessLevel)Convert.ToInt32(value);
            }
            catch (FormatException)
            {
                accessLevel = AccessLevel.VeryHigh;
            }
            // Если настройке присвоено неверное значение, то выставляем максимальный уровень
            if (!Enum.IsDefined(typeof(AccessLevel), accessLevel))
                accessLevel = AccessLevel.VeryHigh;
            return accessLevel;
        }
    }
}