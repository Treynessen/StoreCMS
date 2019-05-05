using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void SetUniqueAliasName(CMSDatabase db, Page page)
        {
            if (page == null)
                throw new ArgumentException();

            if (GetUrl(page).Equals("/admin", StringComparison.InvariantCultureIgnoreCase))
            {
                page.Alias = "admin_page";
            }

            var usualPageUrlsTask = db.UsualPages.AsNoTracking().Where(up => up.GetType() == page.GetType() ? !(up.ID == page.ID) : true)
                .Select(up => GetUrl(up)).ToListAsync();
            var categoryPageUrlsTask = db.CategoryPages.AsNoTracking().Where(cp => cp.GetType() == page.GetType() ? !(cp.ID == page.ID) : true)
                .Select(cp => GetUrl(cp)).ToListAsync();
            var productPageUrlsTask = db.ProductPages.AsNoTracking().Where(pp => pp.GetType() == page.GetType() ? !(pp.ID == page.ID) : true)
                .Select(pp => GetUrl(pp)).ToListAsync();

            CancellationTokenSource categoryPageTokenSource = new CancellationTokenSource();
            CancellationTokenSource productPageTokenSource = new CancellationTokenSource();

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
            string currentPath = GetUrl(page);

            do
            {
                if (categoryPageTokenSource.IsCancellationRequested)
                    categoryPageTokenSource = new CancellationTokenSource();
                if (productPageTokenSource.IsCancellationRequested)
                    productPageTokenSource = new CancellationTokenSource();

                has = false;
                string checkPath = $"{currentPath}{(index == 0 ? "" : index.ToString())}";
                if (index == int.MaxValue)
                {
                    page.Alias += index.ToString();
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
                    page.Alias = GetNameWithUnderscore(page.Alias);
                    currentPath = GetUrl(page);
                }
                if (!has && index != 0)
                {
                    page.Alias += index.ToString();
                }
                ++index;
            } while (has);
        }
    }
}