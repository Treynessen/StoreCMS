using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Treynessen.Extensions;
using Treynessen.LogManagement;
using Treynessen.Database.Context;

namespace Treynessen
{
    public partial class TimedHostedService : IHostedService, IDisposable
    {
        private void TransferLogsFromDbToFiles(CMSDatabase db, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            foreach (var user in db.Users.ToArray())
            {
                db.Entry(user).Collection(u => u.AdminPanelLogs).Load();
                LogManagementFunctions.UserLogsToLogFile(user, DateTime.Now, env.GetLogsFolderFullPath());
                db.Entry(user).State = EntityState.Detached;
                foreach(var log in user.AdminPanelLogs)
                {
                    db.AdminPanelLogs.Remove(log);
                }
            }
            db.SaveChanges();
        }
    }
}