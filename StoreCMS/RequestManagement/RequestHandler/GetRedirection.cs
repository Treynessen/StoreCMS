using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.RequestManagement
{
    public partial class RequestHandler
    {
        public string GetRedirection()
        {
            Redirection redirection = db.Redirections.AsNoTracking().FirstOrDefault(r => r.RequestPathHash == requestStringHash && r.RequestPath.Equals(requestString, StringComparison.Ordinal));
            return redirection?.RedirectionPath;
        }
    }
}