using System;
using Microsoft.Extensions.Configuration;

namespace Treynessen.Security
{
    public class AccessLevelConfiguration
    {
        private IConfiguration configuration;

        public AccessLevelConfiguration(string contentRootPath, string configPath)
        {
            configuration = new ConfigurationBuilder()
                            .SetBasePath(contentRootPath)
                            .AddJsonFile(configPath)
                            .Build();
        }

        public AccessLevel GetAccessLevelInfoTo(string pageName)
        {
            AccessLevel accessLevel;
            string value = configuration[pageName];
            if (string.IsNullOrEmpty(value))
                return AccessLevel.VeryHigh;
            accessLevel = (AccessLevel)Convert.ToInt32(value);
            return accessLevel;
        }
    }
}