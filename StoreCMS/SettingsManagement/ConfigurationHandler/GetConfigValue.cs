namespace Treynessen.SettingsManagement
{
    public partial class ConfigurationHandler
    {
        public string GetConfigValue(string sectionName)
        {
            return configuration[sectionName]?.Replace("\\", "\\\\");
        }
    }
}