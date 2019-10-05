using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.PagesManagement;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        private static Regex ignoredAgents = new Regex("rambler|googlebot|aport|yahoo|msnbot|turtle|mail.ru|omsktele|yetibot|picsearch" +
            "|sape.bot|sape_context|gigabot|snapbot|alexa.com|megadownload.net|askpeter.info|igde.ru|ask.com|qwartabot" +
            "|yanga.co.uk|scoutjet|similarpages|oozbot|shrinktheweb.com|aboutusbot|followsite.com|dataparksearch|google-sitemaps" +
            "|appEngine-google|feedfetcher-google|liveinternet.ru|xml-sitemaps.com|agama|metadatalabs.com|h1.hrn.ru|googlealert.com" +
            "|seo-rus.com|yaDirectBot|yandeG|yandex|yandexSomething|Copyscape.com|AdsBot-Google|domaintools.com|Nigma.ru|bing.com|dotnetdotcom",
            RegexOptions.IgnoreCase);

        [NonAction]
        public void SetVisitInfo(int pageID, PageType pageType)
        {
            if (!ignoredAgents.IsMatch(HttpContext.Request.Headers["User-Agent"]))
            {
                string ip = HttpContext.Connection.RemoteIpAddress.ToString();
                int ipStringHash = OtherFunctions.GetHashFromString(ip);
                Visitor visitor = db.Visitors.FirstOrDefault(v => v.IPStringHash == ipStringHash && v.IPAdress.Equals(ip, StringComparison.Ordinal));
                if (visitor == null)
                {
                    visitor = new Visitor
                    {
                        IPAdress = ip,
                        IPStringHash = ipStringHash,
                        FirstVisit = DateTime.Now,
                        LastVisit = DateTime.Now
                    };
                    db.Visitors.Add(visitor);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateException)
                    {
                        // При одновременных запросах от одного пользователя может быть произведено одновременное добавление посетителя в БД,
                        // поэтому, если выбивает исключение, то отменяем добавление посетителя и производим ещё один поиск в БД
                        db.Entry(visitor).State = EntityState.Detached;
                        visitor = db.Visitors.FirstOrDefault(v => v.IPStringHash == ipStringHash && v.IPAdress.Equals(ip, StringComparison.Ordinal));
                    }
                }
                visitor.LastVisit = DateTime.Now;
                db.VisitedPages.Add(new VisitedPage
                {
                    VisitedPageId = pageID,
                    PageType = pageType,
                    Visitor = visitor
                });
                db.SaveChanges();
            }
        }
    }
}