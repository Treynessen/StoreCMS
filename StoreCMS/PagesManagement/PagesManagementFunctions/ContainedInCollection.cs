using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        private static Task<bool> ContainedInCollection(List<string> collection, string requestPath,
            CancellationToken currentCancellationToken,
            params CancellationTokenSource[] otherCancellationTokenSources)
        {
            return Task.Run(() =>
            {
                foreach (var path in collection)
                {
                    if (currentCancellationToken.IsCancellationRequested)
                        break;
                    if (path.Equals(requestPath, StringComparison.Ordinal))
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
