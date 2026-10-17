using System;
using System.Data.Common;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.LogManagement
{
    public static partial class LogManagementFunctions
    {
        // [Комментарий ИИ-агента]: Только для события после commit: отказ необязательного журнала не отменяет уже успешную операцию и не требует её повтора.
        public static bool TryAddCommittedAdminPanelLog(CMSDatabase db, HttpContext context, string info, User user = null)
        {
            var previous = db.ChangeTracker.Entries<AdminPanelLog>().Select(e => e.Entity).ToArray();
            try
            {
                AddAdminPanelLog(db, context, info, user);
                return true;
            }
            catch (Exception error) when (error is DbUpdateException || error is DbException)
            {
                foreach (var entry in db.ChangeTracker.Entries<AdminPanelLog>().ToArray())
                    if (entry.State == EntityState.Added && !previous.Contains(entry.Entity)) entry.State = EntityState.Detached;
                // [Комментарий ИИ-агента]: В диагностике нет SQL, текста события, исключения или реквизитов; сбой самого провайдера логов также не меняет результат commit.
                try
                {
                    context.RequestServices?.GetService<ILoggerFactory>()?.CreateLogger("StoreCMS.SecurityAudit")
                        .LogError(new EventId(2202, "CommittedAuditFailure"), "Не удалось записать административное событие после успешной фиксации операции.");
                }
                catch (Exception) { }
                return false;
            }
        }
    }
}
