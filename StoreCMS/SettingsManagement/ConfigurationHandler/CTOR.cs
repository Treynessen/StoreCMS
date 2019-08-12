using Microsoft.Extensions.Configuration;

namespace Treynessen.SettingsManagement
{
    public partial class ConfigurationHandler
    {
        private string path;
        private IConfiguration configuration;

        public IConfigurationSection DbConfiguration => configuration.GetSection("DBSettings");
        public IConfigurationSection AccessConfiguration => configuration.GetSection("AdminPanelAccessSettings");

        public ConfigurationHandler(string path)
        {
            configuration = new ConfigurationBuilder().AddJsonFile(path).Build();
            this.path = path;
        }
    }
}