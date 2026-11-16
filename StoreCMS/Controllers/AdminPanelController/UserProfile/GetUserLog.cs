using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult GetUserLog(int? userID, DateTime? currentLogDate, HttpContext context)
        {
            if (!userID.HasValue || !currentLogDate.HasValue)
                return Content(string.Empty);
            IWebHostEnvironment env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            string pathToLogsFolder = $"{env.GetLogsFolderFullPath()}{userID.Value}/";
            if(!System.IO.Directory.Exists(pathToLogsFolder))
                return Content(string.Empty);
            Regex fileNameRegex = new Regex(@"(?<Type1>\d{1,2}).(?<Type2>\d{1,2}).(?<Type3>\d{4}).xml$");
            string fileName = null;
            try
            {
                DateTime fileDate = (from pathToLog in System.IO.Directory.GetFiles(pathToLogsFolder)
                                     let match = fileNameRegex.Match(pathToLog)
                                     where match.Success
                                     let date = new DateTime(Convert.ToInt32(match.Groups[3].Value), Convert.ToInt32(match.Groups[2].Value), Convert.ToInt32(match.Groups[1].Value))
                                     where currentLogDate.Value.CompareTo(date) == 1
                                     select date).Max();
                fileName = $"{fileDate.Day}.{fileDate.Month}.{fileDate.Year}";
                context.Response.Headers.Add("file-date", $"{fileDate.Year}-{fileDate.Month}-{fileDate.Day}");
            }
            catch (InvalidOperationException)
            {
                return Content(string.Empty);
            }
            string logData = OtherFunctions.GetFileContent($"{pathToLogsFolder}{fileName}.xml");
            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.Append($"<p class=\"date\">{fileName}:</p>");
            XDocument document;
            try { document = XDocument.Parse(logData); }
            catch (System.Xml.XmlException) { return Content(string.Empty); }
            foreach (var entry in document.Root.Elements("event"))
            {
                contentBuilder.Append($"<p class=\"log\"><b>{HtmlEncoder.Default.Encode((string)entry.Attribute("time") ?? "")}</b> - {HtmlEncoder.Default.Encode((string)entry.Element("detail") ?? "")}</p>\n");
            }
            return Content(contentBuilder.ToString());
        }
    }
}
