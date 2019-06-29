using Treynessen.Database.Entities;

namespace Treynessen.PagesManagement
{
    public static partial class PagesManagementFunctions
    {
        public static string GetBreadcrumbsHTML(Page page)
        {
            string result = null;
            if (page is UsualPage up)
            {
                if (up.PreviousPage == null)
                    result = string.Empty;
                else if (string.IsNullOrEmpty(up.PreviousPage.BreadcrumbsHtml))
                    result = $"<a href=\"{GetUrl(up.PreviousPage)}\">{up.PreviousPage.PageName}</a>";
                else
                    result = $"{up.PreviousPage.BreadcrumbsHtml} → <a href=\"{GetUrl(up.PreviousPage)}\">{up.PreviousPage.PageName}</a>";
            }
            else if (page is CategoryPage cp)
            {
                if (cp.PreviousPage == null)
                    result = string.Empty;
                else if (string.IsNullOrEmpty(cp.PreviousPage.BreadcrumbsHtml))
                    result = $"<a href=\"{GetUrl(cp.PreviousPage)}\">{cp.PreviousPage.PageName}</a>";
                else
                    result = $"{cp.PreviousPage.BreadcrumbsHtml} → <a href=\"{GetUrl(cp.PreviousPage)}\">{cp.PreviousPage.PageName}</a>";
            }
            else if (page is ProductPage pp)
            {
                if (pp.PreviousPage == null)
                    result = string.Empty;
                else if (string.IsNullOrEmpty(pp.PreviousPage.BreadcrumbsHtml))
                    result = $"<a href=\"{GetUrl(pp.PreviousPage)}\">{pp.PreviousPage.PageName}</a>";
                else
                    result = $"{pp.PreviousPage.BreadcrumbsHtml} → <a href=\"{GetUrl(pp.PreviousPage)}\">{pp.PreviousPage.PageName}</a>";
            }
            return result;
        }
    }
}