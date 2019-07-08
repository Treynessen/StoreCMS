using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Treynessen.Controllers;

namespace Treynessen.TagHelpers
{
    public class CostSortTagHelper : TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext Context { get; set; }

        public string AscendingPriceText { get; set; }
        public string DescendingPriceText { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            OrderBy? currentOrderBy = Context.HttpContext.Items["OrderBy"] == null ? null : (OrderBy?)Context.HttpContext.Items["OrderBy"];
            int? currentPage = Context.HttpContext.Items["CurrentPage"] == null ? null : (int?)Context.HttpContext.Items["CurrentPage"];
            if (currentPage.HasValue && currentPage.Value == 1)
                currentPage = null;
            string currentPath = Context.HttpContext.Request.Path;

            output.TagName = "a";
            string tagContent = null;
            OrderBy newOrderBy = OrderBy.descending_price;
            if (!currentOrderBy.HasValue || currentOrderBy.Value == OrderBy.ascending_price)
            {
                tagContent = DescendingPriceText;
            }
            else
            {
                tagContent = AscendingPriceText;
                newOrderBy = OrderBy.ascending_price;
            }
            string parameters = currentPage.HasValue ? $"?page={currentPage.Value}" : string.Empty;
            if (string.IsNullOrEmpty(parameters))
                parameters = $"?orderby={newOrderBy}";
            else parameters += $"&orderby={newOrderBy}";
            output.Attributes.Add("href", $"{currentPath}{parameters}");
            output.Content.AppendHtml(tagContent);
        }
    }
}