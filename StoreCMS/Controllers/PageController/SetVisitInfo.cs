using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.PagesManagement;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        private static Regex correctUserAgents = new Regex("(?<!(bot|favicon|compatible)(.*)?)(windows nt|linux|android|iphone|mac|os x)(?!(.*)?(bot|favicon|compatible))", RegexOptions.IgnoreCase);

        [NonAction]
        public void SetVisitInfo(int pageID, PageType pageType)
        {
            Visitor visitor = null;
            // Если в кукисах пользователя хранится информация о последнем заходе, тогда проверяем её
            // Если она равна текущей дате, тогда выполняем поиск посетителя по ID
            if (HttpContext.Request.Cookies.ContainsKey("VisitorID")
                && HttpContext.Request.Cookies.ContainsKey("LastVisit")
                && HttpContext.Request.Cookies["LastVisit"].Equals(DateTime.Now.ToShortDateString()))
            {
                try
                {
                    int visitorID = Convert.ToInt32(HttpContext.Request.Cookies["VisitorID"]);
                    visitor = db.Visitors.FirstOrDefault(v => v.ID == visitorID);
                }
                catch { }
            }
            // Если у пользователя нет информации о последнем визите или, если его ID не был найден в БД, тогда
            // ищем пользователя по IP
            if (visitor == null && correctUserAgents.IsMatch(HttpContext.Request.Headers["User-Agent"]))
            {
                string ip = HttpContext.Connection.RemoteIpAddress.ToString();
                int ipStringHash = OtherFunctions.GetHashFromString(ip);
                visitor = db.Visitors.FirstOrDefault(v => v.IPStringHash == ipStringHash && v.IPAdress.Equals(ip, StringComparison.Ordinal));
                // Если пользователь не найден, тогда добавляем его
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
                DateTime dtNow = DateTime.Now;
                DateTime nextDay = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day).AddDays(1);
                CookieOptions cookieOptions = new CookieOptions { MaxAge = nextDay - dtNow };
                HttpContext.Response.Cookies.Append("VisitorID", visitor.ID.ToString(), cookieOptions);
                HttpContext.Response.Cookies.Append("LastVisit", DateTime.Now.ToShortDateString().ToString(), cookieOptions);
            }
            if (visitor != null)
            {
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