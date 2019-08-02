using System;
using Microsoft.AspNetCore.Http;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.LogManagement
{
    public static partial class LogManagementFunctions
    {
        public static void AddAdminPanelLog(CMSDatabase db, HttpContext context, string info, User user = null)
        {
            if (user == null)
                user = context.Items["User"] as User;
            db.AdminPanelLogs.Add(new AdminPanelLog
            {
                Info = info,
                Time = DateTime.Now,
                User = user
            });
            db.SaveChanges();
        }
    }
}