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
        private static readonly object sync = new object();

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
                try
                {
                    var request = accessor.HttpContext?.Request;
                    string origin = request == null ? "BACKGROUND" : $"FROM {request.Scheme}://{request.Host}{request.Path} {request.Method} REQUEST";
                    lock (sync)
                    using (StreamWriter writer = new StreamWriter(path, true))
                        writer.WriteLine($"{DateTime.Now} - {origin}:\n{formatter(state, exception)}\n");
                }
                catch (IOException) { Console.Error.WriteLine("StoreCMS: не удалось записать журнал ошибок."); }
                catch (UnauthorizedAccessException) { Console.Error.WriteLine("StoreCMS: доступ к журналу ошибок запрещён."); }
            }
        }
    }
}
