using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult GetUserLog(int? userID, DateTime? currentLogDate, HttpContext context)
        {
            if (!userID.HasValue || !currentLogDate.HasValue)
                return Content(string.Empty);
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string pathToLogsFolder = $"{env.GetLogsFolderFullPath()}{userID.Value}\\";
            if(!System.IO.Directory.Exists(pathToLogsFolder))
                return Content(string.Empty);
            Regex fileNameRegex = new Regex(@"(?<Type1>\d{1,2}).(?<Type2>\d{1,2}).(?<Type3>\d{4}).xml$");
            DateTime[] dates = (from pathToLog in System.IO.Directory.GetFiles(pathToLogsFolder)
                                 let match = fileNameRegex.Match(pathToLog)
                                 where match.Success
                                 let date = new DateTime(Convert.ToInt32(match.Groups[3].Value), Convert.ToInt32(match.Groups[2].Value), Convert.ToInt32(match.Groups[1].Value))
                                 where currentLogDate.Value.CompareTo(date) == 1
                                 select date).OrderByDescending(t => t.Date).ToArray();
            string fileName = dates.Length > 0 ? $"{dates[0].Day}.{dates[0].Month}.{dates[0].Year}" : string.Empty;
            if (string.IsNullOrEmpty(fileName))
                return Content(string.Empty);
            context.Response.Headers.Add("file-date", $"{dates[0].Year}-{dates[0].Month}-{dates[0].Day}");
            string logData = OtherFunctions.GetFileContent($"{pathToLogsFolder}{fileName}.xml");
            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.Append($"<p class=\"date\">{fileName}:</p>");
            Regex logRegex = new Regex("<event time=\"(?<Type1>\\d{2}:\\d{2})\"><detail>(?<Type2>.*)</detail></event>");
            foreach (var match in logRegex.Matches(logData) as IEnumerable<Match>)
            {
                contentBuilder.Append($"<p class=\"log\"><b>{match.Groups[1].Value}</b> - {match.Groups[2].Value}</p>\n");
            }
            return Content(contentBuilder.ToString());
        }
    }
}