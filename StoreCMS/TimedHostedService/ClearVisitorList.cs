using System;
using Microsoft.Extensions.Hosting;
using Treynessen.Database.Context;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private void ClearVisitorList(CMSDatabase db)
        {
            foreach (var visitor in db.Visitors)
            {
                db.Visitors.Remove(visitor);
            }
            db.SaveChanges();
        }
    }
}