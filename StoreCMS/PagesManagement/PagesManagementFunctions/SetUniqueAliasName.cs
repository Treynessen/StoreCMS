using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Treynessen.Functions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static void SetUniqueAliasName(CMSDatabase db, Page page)
        {
            if (page == null)
                throw new ArgumentException();

            var usualPageUrlsTask = db.UsualPages.AsNoTracking()
                .Where(up => up.GetType() == page.GetType() ? up.ID != page.ID : true)
                .Select(up => up.RequestPath).ToListAsync();

            var categoryPageUrlsTask = db.CategoryPages.AsNoTracking()
                .Where(cp => cp.GetType() == page.GetType() ? cp.ID != page.ID : true)
                .Select(cp => cp.RequestPath).ToListAsync();

            var productPageUrlsTask = db.ProductPages.AsNoTracking()
                .Where(pp => pp.GetType() == page.GetType() ? pp.ID != page.ID : true)
                .Select(pp => pp.RequestPath).ToListAsync();

            Func<string, List<string>, CancellationToken?, Task<bool>> hasSimilarUrlInCollection =
                async (url, collection, token) =>
                {
                    if (collection == null || collection.Count == 0)
                        return false;
                    return await Task.Run(() =>
                    {
                        foreach (var c in collection)
                            if (!token.HasValue || !token.Value.IsCancellationRequested)
                                if (c.Equals(url, StringComparison.InvariantCultureIgnoreCase))
                                    return true;
                        return false;
                    });
                };

            int index = 0;
            bool has = false;
            string currentPath = page.RequestPath;

            do
            {
                CancellationTokenSource categoryPageTokenSource = new CancellationTokenSource();
                CancellationTokenSource productPageTokenSource = new CancellationTokenSource();

                has = false;
                string checkPath = $"{currentPath}{(index == 0 ? string.Empty : index.ToString())}";
                if (index == int.MaxValue)
                {
                    page.Alias += index.ToString();
                    currentPath = page.RequestPath;
                    index = 1;
                }

                var hasInUsualPagesTask = hasSimilarUrlInCollection(checkPath, usualPageUrlsTask.Result, null);
                var hasInCategoryPagesTask = hasSimilarUrlInCollection(checkPath, categoryPageUrlsTask.Result, categoryPageTokenSource.Token);
                var hasInProductPagesTask = hasSimilarUrlInCollection(checkPath, productPageUrlsTask.Result, productPageTokenSource.Token);
                if (hasInUsualPagesTask.Result)
                {
                    has = true;
                    categoryPageTokenSource.Cancel();
                    productPageTokenSource.Cancel();
                }
                else if (hasInCategoryPagesTask.Result)
                {
                    has = true;
                    productPageTokenSource.Cancel();
                }
                else if (hasInProductPagesTask.Result) has = true;

                if (has && index == 0)
                {
                    page.RequestPath = page.RequestPath.Substring(0, page.RequestPath.Length - page.Alias.Length);
                    page.Alias = OtherFunctions.GetNameWithUnderscore(page.Alias);
                    page.RequestPath += page.Alias;
                    currentPath = page.RequestPath;
                }
                if (!has && index != 0)
                {
                    page.Alias += index.ToString();
                    page.RequestPath = currentPath + index.ToString();
                }
                ++index;
                categoryPageTokenSource.Dispose();
                productPageTokenSource.Dispose();
            } while (has);
        }
    }
}