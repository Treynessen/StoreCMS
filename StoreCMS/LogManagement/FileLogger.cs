using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Treynessen.LogManagement
{
    public class FileLogger : ILogger
    {
        private string path;
        private IHttpContextAccessor accessor;
        private static Mutex mutex = new Mutex();

        public FileLogger(string path, IHttpContextAccessor accessor)
        {
            this.path = path;
            this.accessor = accessor;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            {
                return true;
            }
            else return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var request = accessor.HttpContext.Request;
                mutex.WaitOne();
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine($"FROM {request.Scheme}://{request.Host}{request.Path} {request.Method} REQUEST: {formatter(state, exception)}");
                }
                mutex.ReleaseMutex();
            }
        }
    }
}
