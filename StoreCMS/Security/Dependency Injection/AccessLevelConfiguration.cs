using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Treynessen.Security
{
    public class AccessLevelConfiguration
    {
        private IConfiguration configuration;
        private Dictionary<string, AccessLevel> accessLevels;

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
            if (accessLevels == null || !accessLevels.TryGetValue(pageName, out accessLevel))
            {
                string value = configuration[pageName];
                if (string.IsNullOrEmpty(value))
                    return AccessLevel.VeryHigh;
                accessLevel = (AccessLevel)Convert.ToInt32(value);
                if (accessLevels == null)
                    accessLevels = new Dictionary<string, AccessLevel>();
                accessLevels[pageName] = accessLevel;
            }
            return accessLevel;
        }
    }
}