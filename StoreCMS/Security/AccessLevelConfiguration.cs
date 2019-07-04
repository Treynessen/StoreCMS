using System;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Treynessen.AdminPanelTypes;
using Treynessen.SettingsManagement;

namespace Treynessen.Security
{
    public class AccessLevelConfiguration
    {
        private ConfigurationHandler configurationHandler;

        public AccessLevelConfiguration(ConfigurationHandler configurationHandler)
        {
            this.configurationHandler = configurationHandler;
        }

        public AccessLevel GetAccessLevelInfoTo(string pageName)
        {
            AccessLevel accessLevel;
            string value = configurationHandler.Configuration[$"AdminPanelAccessSettings:{pageName}"];
            // Если настройка не определена в конфигурации, то выставляем ей максимальный уровень доступа
            if (string.IsNullOrEmpty(value))
                return AccessLevel.VeryHigh;
            accessLevel = (AccessLevel)Convert.ToInt32(value);
            return accessLevel;
        }
    }
}