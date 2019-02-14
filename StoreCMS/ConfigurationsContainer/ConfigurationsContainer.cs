using Microsoft.Extensions.Configuration;

namespace Trane.Configurations
{
    public class ConfigurationsContainer
    {
        public IConfiguration CoreConfiguration { get; }
        public IConfiguration PathsConfiguration { get; }

        public ConfigurationsContainer(IConfiguration core_configuration,
            IConfiguration paths_configuration)
        {
            CoreConfiguration = core_configuration;
            PathsConfiguration = paths_configuration;
        }
    }
}
