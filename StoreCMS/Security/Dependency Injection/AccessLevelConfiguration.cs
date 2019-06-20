using System;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Treynessen.AdminPanelTypes;

namespace Treynessen.Security
{
    public class AccessLevelConfiguration
    {
        private IConfiguration configuration;
        private string configPath;

        public AccessLevelConfiguration(string contentRootPath, string configPath)
        {
            configuration = new ConfigurationBuilder()
                            .SetBasePath(contentRootPath)
                            .AddJsonFile(configPath)
                            .Build();
            this.configPath = $"{contentRootPath}/{configPath}";
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

        public void ReplaceJsonWithSettingModel(AccessSettingsModel model)
        {
            StringBuilder configContentBuilder = new StringBuilder();
            configContentBuilder.Append("{\n");
            PropertyInfo[] modelProperties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            int veryLowLevelValue = (int)AccessLevel.VeryLow;
            int veryHighLevelValue = (int)AccessLevel.VeryHigh;
            for (int i = 0; i < modelProperties.Length; ++i)
            {
                int accessLevel = (int)modelProperties[i].GetValue(model);
                if (accessLevel < veryLowLevelValue || accessLevel > veryHighLevelValue)
                    accessLevel = veryHighLevelValue;
                configContentBuilder.Append($"\t\"{modelProperties[i].Name}\": \"{accessLevel}\"");
                if (i != modelProperties.Length - 1)
                    configContentBuilder.Append(",\n");
                else configContentBuilder.Append("\n");
            }
            configContentBuilder.Append("}");
            using (StreamWriter writer = new StreamWriter(configPath, false))
            {
                writer.Write(configContentBuilder.ToString());
            }
        }
    }
}