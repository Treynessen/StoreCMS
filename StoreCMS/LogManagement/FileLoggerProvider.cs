using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Treynessen.LogManagement
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private string path;
        private IHttpContextAccessor accessor;

        public FileLoggerProvider(string path, IHttpContextAccessor accessor)
        {
            this.path = path;
            this.accessor = accessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(path, accessor);
        }

        public void Dispose()
        {
            
        }
    }
}
