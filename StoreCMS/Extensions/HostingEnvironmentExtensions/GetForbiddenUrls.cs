using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;

namespace Treynessen.Extensions
{
    public static partial class HostingEnvironmentExtensions
    {
        private static LinkedList<string> forbiddenUrls;
        public static LinkedList<string> GetForbiddenUrls(this IHostingEnvironment env)
        {
            if (forbiddenUrls == null)
                forbiddenUrls = new LinkedList<string>(new[]{ "/admin", "/search" });
            return forbiddenUrls;
        }
    }
}