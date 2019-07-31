using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.RequestManagement
{
    public partial class RequestHandler
    {
        public Page FindPage()
        {
            string _requestString = requestString;
            if (_requestString.Length > 1 && _requestString[0].Equals('/'))
                _requestString = _requestString.Substring(1);
            string[] splitedRequestString = _requestString.Split('/');

            Page page = null;
            if (splitedRequestString.Length != 1)
            {
                page = db.ProductPages.AsNoTracking().FirstOrDefault(pp => pp.RequestPathHash == requestStringHash && pp.RequestPath.Equals(requestString, StringComparison.Ordinal));
            }
            if (page == null)
            {
                page = db.CategoryPages.AsNoTracking().FirstOrDefault(cp => cp.RequestPathHash == requestStringHash && cp.RequestPath.Equals(requestString, StringComparison.Ordinal));
            }
            if (page == null)
            {
                page = db.UsualPages.AsNoTracking().FirstOrDefault(up => up.RequestPathHash == requestStringHash && up.RequestPath.Equals(requestString, StringComparison.Ordinal));
            }
            return page;
        }
    }
}