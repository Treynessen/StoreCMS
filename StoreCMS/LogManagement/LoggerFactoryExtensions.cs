using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Treynessen.LogManagement
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddFile(this ILoggerFactory factory, string pathToFolder, string fileFullName, IHttpContextAccessor accessor)
        {
            if (!Directory.Exists(pathToFolder))
            {
                Directory.CreateDirectory(pathToFolder);
            }
            factory.AddProvider(new FileLoggerProvider(pathToFolder + fileFullName, accessor));
            return factory;
        }
    }
}
