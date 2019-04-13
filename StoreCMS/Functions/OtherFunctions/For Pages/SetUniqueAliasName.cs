using System;
using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static void SetUniqueAliasName(CMSDatabase db, Page page)
        {
            int index = 0;
            bool has = false;
            string currentPath = GetUrl(page);
            do
            {
                string checkPath = $"{currentPath}{(index == 0 ? "" : index.ToString())}";
                has = false;
                if (index == int.MaxValue)
                {
                    page.Alias += index.ToString();
                    index = 1;
                }
                var UsualPage = db.UsualPages.FirstOrDefaultAsync(sp => $"{GetUrl(sp)}".Equals(checkPath) && (sp.GetType() == page.GetType() ? sp.ID != page.ID : true));
                var categoryPage = db.CategoryPages.FirstOrDefaultAsync(cp => $"{GetUrl(cp)}".Equals(checkPath) && (cp.GetType() == page.GetType() ? cp.ID != page.ID : true));
                var productPage = db.ProductPages.FirstOrDefaultAsync(pp => $"{GetUrl(pp)}".Equals(checkPath) && (pp.GetType() == page.GetType() ? pp.ID != page.ID : true));
                if (UsualPage.Result != null) has = true;
                else if (categoryPage.Result != null) has = true;
                else if (productPage.Result != null) has = true;
                if (has && index == 0)
                {
                    try
                    {
                        int lastUnderscore = page.Alias.LastIndexOf('_'); 
                        if (lastUnderscore + 1 != page.Alias.Length)
                        {
                            if (lastUnderscore == -1)
                                throw new FormatException();
                            Convert.ToInt32(page.Alias.Substring(lastUnderscore + 1));
                            page.Alias = page.Alias.Substring(0, lastUnderscore + 1);
                            currentPath = GetUrl(page);
                        }
                    }
                    catch (FormatException)
                    {
                        page.Alias += "_";
                        currentPath += "_";
                    }
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