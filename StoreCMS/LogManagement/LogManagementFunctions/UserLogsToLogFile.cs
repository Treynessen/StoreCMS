using System;
using System.IO;
using System.Text;
using Treynessen.Database.Entities;

namespace Treynessen.LogManagement
{
    public static partial class LogManagementFunctions
    {
        public static void UserLogsToLogFile(User user, DateTime currentDay, string pathToLogsFolder)
        {
            string pathToUserLogs = $"{pathToLogsFolder}{user.ID.ToString()}/";
            if (!Directory.Exists(pathToUserLogs))
                Directory.CreateDirectory(pathToUserLogs);
            DateTime previousDay = DateTime.Now.AddDays(-1);
            if (user.AdminPanelLogs.Count > 0)
            {
                using (StreamWriter writer = new StreamWriter($"{pathToUserLogs}{previousDay.Day.ToString()}.{previousDay.Month.ToString()}.{previousDay.Year.ToString()}.xml"))
                {
                    StringBuilder contentBuilder = new StringBuilder("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n<log>\n");
                    foreach(var log in user.AdminPanelLogs)
                    {
                        contentBuilder.Append($"\t<event time=\"{log.Time.ToShortTimeString()}\"><detail>{log.Info}</detail></event>\n");
                    }
                    contentBuilder.Append("</log>");
                    writer.Write(contentBuilder.ToString());
                }
            }
        }
    }
}