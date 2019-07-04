using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Treynessen.SettingsManagement
{
    public partial class ConfigurationHandler
    {
        public string Path { get; private set; }
        public IConfiguration Configuration { get; private set; }

        public ConfigurationHandler(string path, IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile(path).Build();
            Path = path;
        }
    }
}