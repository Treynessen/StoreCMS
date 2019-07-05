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

            int index = 0;
            bool has = false;
            string currentPath = page.RequestPath;

            do
            {
                CancellationTokenSource usualPageTokenSource = new CancellationTokenSource();
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

                var hasInUsualPagesTask = ContainedInCollection(usualPageUrlsTask.Result, checkPath, usualPageTokenSource, categoryPageTokenSource, productPageTokenSource);
                var hasInCategoryPagesTask = ContainedInCollection(categoryPageUrlsTask.Result, checkPath, categoryPageTokenSource, usualPageTokenSource, productPageTokenSource);
                var hasInProductPagesTask = ContainedInCollection(productPageUrlsTask.Result, checkPath, productPageTokenSource, usualPageTokenSource, categoryPageTokenSource);

                if (hasInUsualPagesTask.Result || hasInCategoryPagesTask.Result || hasInProductPagesTask.Result)
                    has = true;

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
                usualPageTokenSource.Dispose();
                categoryPageTokenSource.Dispose();
                productPageTokenSource.Dispose();
            } while (has);
        }
    }
}