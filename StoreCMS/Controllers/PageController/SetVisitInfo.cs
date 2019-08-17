using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.PagesManagement;
using Treynessen.Database.Entities;

namespace Treynessen.Controllers
{
    public partial class PageController : Controller
    {
        [NonAction]
        public void SetVisitInfo(int pageID, PageType pageType)
        {
            bool hasCookieInfo = false;
            Visitor visitor = null;
            string ip = HttpContext.Connection.RemoteIpAddress.ToString();
            int ipStringHash = OtherFunctions.GetHashFromString(ip);
            string logIDKeyInCookie = $"log_id-{DateTime.Now.Year}{DateTime.Now.Month}{DateTime.Now.Day}";
            try
            {
                int logID = Convert.ToInt32(HttpContext.Request.Cookies[logIDKeyInCookie]);
                visitor = db.Visitors.FirstOrDefault(v => v.ID == logID);
                if (visitor == null)
                    throw new NullReferenceException();
                hasCookieInfo = true;
            }
            catch
            {
                visitor = db.Visitors.FirstOrDefault(v => v.IPStringHash == ipStringHash && v.IPAdress.Equals(ip, StringComparison.Ordinal));
            }
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
                    db.Entry(visitor).State = EntityState.Detached;
                    // При одновременных запросах от одного пользователя может быть произведено одновременное добавление посетителя в БД,
                    // поэтому, если выбивает исключение, то отменяем добавление посетителя и производим ещё один поиск в БД
                    visitor = db.Visitors.FirstOrDefault(v => v.IPStringHash == ipStringHash && v.IPAdress.Equals(ip, StringComparison.Ordinal));
                }
            }
            else
            {
                visitor.LastVisit = DateTime.Now;
            }
            db.VisitedPages.Add(new VisitedPage
            {
                VisitedPageId = pageID,
                PageType = pageType,
                Visitor = visitor
            });
            db.SaveChanges();
            if (!hasCookieInfo)
                HttpContext.Response.Cookies.Append(logIDKeyInCookie, visitor.ID.ToString());
        }
    }
}