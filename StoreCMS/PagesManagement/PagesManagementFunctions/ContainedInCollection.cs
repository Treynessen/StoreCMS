using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        private static Task<bool> ContainedInCollection(List<string> collection, string requestPath,
            CancellationTokenSource currentCancellationTokenSource,
            params CancellationTokenSource[] otherCancellationTokenSources)
        {
            CancellationToken token = currentCancellationTokenSource.Token;
            return Task.Run(() =>
            {
                foreach (var path in collection)
                {
                    if (token.IsCancellationRequested)
                        break;
                    if (path.Equals(requestPath, StringComparison.InvariantCulture))
                    {
                        if (otherCancellationTokenSources != null)
                        {
                            foreach (var t in otherCancellationTokenSources)
                                t?.Cancel();
                        }
                        return true;
                    }
                }
                return false;
            });
        }
    }
}
