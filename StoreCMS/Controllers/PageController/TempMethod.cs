using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        private static Mutex mutex = new Mutex();
        private static void TempMethod(HttpContext context, string userAgent, string userIp)
        {
            var env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            Task.Run(() =>
            {
                mutex.WaitOne();
                try
                {
                    if (!Directory.Exists(env.GetLogsFolderFullPath()))
                    {
                        Directory.CreateDirectory(env.GetLogsFolderFullPath());
                    }
                    using (StreamWriter writer = new StreamWriter($"{env.GetLogsFolderFullPath()}visitors.txt", true))
                    {
                        writer.WriteLine($"{DateTime.Now} - {userIp} - {userAgent}");
                    }
                    mutex.ReleaseMutex();
                }
                catch
                {
                    mutex.ReleaseMutex();
                }
            });
        }
    }
}