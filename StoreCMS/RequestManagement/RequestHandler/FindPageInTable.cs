using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.RequestManagement
{
    public partial class RequestHandler
    {
        private Task<T> FindPageInTable<T>(DbSet<T> table,
            CancellationTokenSource currentCancellationTokenSource,
            params CancellationTokenSource[] otherCancellationTokenSources)
            where T : Page
        {
            CancellationToken token = currentCancellationTokenSource.Token;
            return Task.Run(() =>
            {
                try
                {
                    T page = table.FirstOrDefaultAsync(p => p.RequestPath.Equals(requestString, StringComparison.InvariantCultureIgnoreCase), token).Result;
                    if (page != null && otherCancellationTokenSources != null)
                        foreach (var t in otherCancellationTokenSources)
                            t?.Cancel();
                    return page;
                }
                catch (AggregateException)
                {
                    return null;
                }
            });
        }
    }
}