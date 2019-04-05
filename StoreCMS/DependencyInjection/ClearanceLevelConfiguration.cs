using System;
using Trane.Db.Entities.TypesForEntities;
using Microsoft.Extensions.Configuration;

namespace Trane.Configurations
{
    public class ClearanceLevelConfiguration
    {
        private IConfigurationSection clearanceLevelSettings;
        private ClearanceLevel? showMainPage;
        private ClearanceLevel? showPages;
        private ClearanceLevel? addPage;
        private ClearanceLevel? accessToSettings;

        public ClearanceLevelConfiguration(IConfigurationSection clearanceLevelSettings)
        {
            this.clearanceLevelSettings = clearanceLevelSettings;
        }

        public ClearanceLevel ShowMainPage
        {
            get
            {
                if (!showMainPage.HasValue)
                    showMainPage = (ClearanceLevel)Convert.ToInt32(clearanceLevelSettings["ShowMainPage"]);
                return showMainPage.Value;
            }
        }

        public ClearanceLevel ShowPages
        {
            get
            {
                if (!showPages.HasValue)
                    showPages = (ClearanceLevel)Convert.ToInt32(clearanceLevelSettings["ShowPages"]);
                return showPages.Value;
            }
        }

        public ClearanceLevel AddPage
        {
            get
            {
                if (!addPage.HasValue)
                    showPages = (ClearanceLevel)Convert.ToInt32(clearanceLevelSettings["AddPage"]);
                return showPages.Value;
            }
        }

        public ClearanceLevel AccessToSettings
        {
            get
            {
                if (!accessToSettings.HasValue)
                    accessToSettings = (ClearanceLevel)Convert.ToInt32(clearanceLevelSettings["AccessToSettings"]);
                return accessToSettings.Value;
            }
        }
    }
}
