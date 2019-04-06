using System;
using Microsoft.Extensions.Configuration;
using Trane.OtherTypes;

namespace Trane.DependencyInjection
{
    public class AccessLevelConfiguration
    {
        private IConfigurationSection clearanceLevelSettings;
        private AccessLevels? showMainPage;
        private AccessLevels? showPages;
        private AccessLevels? addPage;
        private AccessLevels? accessToSettings;

        public AccessLevelConfiguration(IConfigurationSection clearanceLevelSettings)
        {
            this.clearanceLevelSettings = clearanceLevelSettings;
        }

        public AccessLevels ShowMainPage
        {
            get
            {
                if (!showMainPage.HasValue)
                    showMainPage = (AccessLevels)Convert.ToInt32(clearanceLevelSettings["ShowMainPage"]);
                return showMainPage.Value;
            }
        }

        public AccessLevels ShowPages
        {
            get
            {
                if (!showPages.HasValue)
                    showPages = (AccessLevels)Convert.ToInt32(clearanceLevelSettings["ShowPages"]);
                return showPages.Value;
            }
        }

        public AccessLevels AddPage
        {
            get
            {
                if (!addPage.HasValue)
                    addPage = (AccessLevels)Convert.ToInt32(clearanceLevelSettings["AddPage"]);
                return addPage.Value;
            }
        }

        public AccessLevels AccessToSettings
        {
            get
            {
                if (!accessToSettings.HasValue)
                    accessToSettings = (AccessLevels)Convert.ToInt32(clearanceLevelSettings["AccessToSettings"]);
                return accessToSettings.Value;
            }
        }
    }
}
