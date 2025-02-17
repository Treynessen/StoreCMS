#pragma checksum "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "62b556c1a7162b1e860574b40d3502cd1d71f581"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_AdminPanel_CategoriesAndProducts_CategoryProducts), @"mvc.1.0.view", @"/Views/AdminPanel/CategoriesAndProducts/CategoryProducts.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/AdminPanel/CategoriesAndProducts/CategoryProducts.cshtml", typeof(AspNetCore.Views_AdminPanel_CategoriesAndProducts_CategoryProducts))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Security;

#line default
#line hidden
#line 2 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Localization;

#line default
#line hidden
#line 3 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.PagesManagement;

#line default
#line hidden
#line 4 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.AdminPanelTypes;

#line default
#line hidden
#line 5 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 6 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
#line 1 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
using Microsoft.AspNetCore.Hosting;

#line default
#line hidden
#line 2 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
using Treynessen.Extensions;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"62b556c1a7162b1e860574b40d3502cd1d71f581", @"/Views/AdminPanel/CategoriesAndProducts/CategoryProducts.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"643dee66794a2698b58719ead6ad3b58418e7a0e", @"/Views/AdminPanel/_ViewImports.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"cd484aca00b6366db4c9613599bf6cd4bf9f84d6", @"/Views/AdminPanel/CategoriesAndProducts/_ViewImports.cshtml")]
    public class Views_AdminPanel_CategoriesAndProducts_CategoryProducts : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<List<ProductPage>>
    {
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Treynessen.TagHelpers.ImageTagHelper __Treynessen_TagHelpers_ImageTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 6 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
  
    Layout = "CommonPage";
    Context.Items["PageStyle"] = "/styles/admin_panel/categories_and_products/category_products.css";
    int categoryID = (int)Context.Items["itemID"];
    string categoryName = Context.Items["categoryName"] as string;
    Context.Items["PageName"] = $"{localization.CategoryProductsPageName} {categoryName}";
    User user = Context.Items["User"] as User;

#line default
#line hidden
            BeginContext(583, 144, true);
            WriteLiteral("<script>\r\n                function responseHandler(request) {\r\n                    if (request.status == 200) {\r\n                        alert(\'");
            EndContext();
            BeginContext(728, 37, false);
#line 17 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                          Write(Html.Raw(localization.ProductDeleted));

#line default
#line hidden
            EndContext();
            BeginContext(765, 158, true);
            WriteLiteral("\');\r\n                        location.reload();\r\n                    }\r\n                    else if (request.status == 404) {\r\n                        alert(\'");
            EndContext();
            BeginContext(924, 38, false);
#line 21 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                          Write(Html.Raw(localization.ProductNotFound));

#line default
#line hidden
            EndContext();
            BeginContext(962, 70, true);
            WriteLiteral("\');\r\n                    }\r\n                }\r\n            </script>\r\n");
            EndContext();
#line 25 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
 if (SecurityFunctions.HasAccessTo(AdminPanelPages.AddProduct, user, Context))
{

#line default
#line hidden
            BeginContext(1115, 64, true);
            WriteLiteral("            <div class=\"add-product-button\">\r\n                <a");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 1179, "\"", 1268, 5);
#line 28 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 1186, Context.Request.Path, 1186, 21, false);

#line default
#line hidden
            WriteAttributeValue("", 1207, "?pageID=", 1207, 8, true);
#line 28 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 1215, (int)AdminPanelPages.AddProduct, 1215, 34, false);

#line default
#line hidden
            WriteAttributeValue("", 1249, "&itemID=", 1249, 8, true);
#line 28 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 1257, categoryID, 1257, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1269, 1, true);
            WriteLiteral(">");
            EndContext();
            BeginContext(1271, 70, false);
#line 28 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                                                        Write(Html.Raw($"{localization.AddProductInCategory} <b>{categoryName}</b>"));

#line default
#line hidden
            EndContext();
            BeginContext(1341, 26, true);
            WriteLiteral("</a>\r\n            </div>\r\n");
            EndContext();
#line 30 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
}

#line default
#line hidden
            BeginContext(1370, 98, true);
            WriteLiteral("            <table>\r\n                <tr>\r\n                    <td></td>\r\n                    <td>");
            EndContext();
            BeginContext(1469, 27, false);
#line 34 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw(localization.Name));

#line default
#line hidden
            EndContext();
            BeginContext(1496, 65, true);
            WriteLiteral("</td>\r\n                    <td>URL</td>\r\n                    <td>");
            EndContext();
            BeginContext(1562, 30, false);
#line 36 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw(localization.Actions));

#line default
#line hidden
            EndContext();
            BeginContext(1592, 30, true);
            WriteLiteral("</td>\r\n                </tr>\r\n");
            EndContext();
#line 38 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 foreach (var p in Model)
                {

#line default
#line hidden
            BeginContext(1684, 46, true);
            WriteLiteral("                <tr>\r\n                    <td>");
            EndContext();
            BeginContext(1730, 136, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("image", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "62b556c1a7162b1e860574b40d3502cd1d71f5819985", async() => {
            }
            );
            __Treynessen_TagHelpers_ImageTagHelper = CreateTagHelper<global::Treynessen.TagHelpers.ImageTagHelper>();
            __tagHelperExecutionContext.Add(__Treynessen_TagHelpers_ImageTagHelper);
            BeginWriteTagHelperAttribute();
#line 41 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                        WriteLiteral(Html.Raw($"{env.GetProductsImagesFolderSrc()}{p.ID}/{p.Alias}.jpg"));

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Treynessen_TagHelpers_ImageTagHelper.Src = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("src", __Treynessen_TagHelpers_ImageTagHelper.Src, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 41 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
__Treynessen_TagHelpers_ImageTagHelper.MaxHeight = 150;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("max-height", __Treynessen_TagHelpers_ImageTagHelper.MaxHeight, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 41 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
__Treynessen_TagHelpers_ImageTagHelper.MaxWidth = 150;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("max-width", __Treynessen_TagHelpers_ImageTagHelper.MaxWidth, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 41 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
__Treynessen_TagHelpers_ImageTagHelper.Quality = 50;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("quality", __Treynessen_TagHelpers_ImageTagHelper.Quality, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(1866, 31, true);
            WriteLiteral("</td>\r\n                    <td>");
            EndContext();
            BeginContext(1898, 54, false);
#line 42 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw($"{p.PageName} <b>(ID{p.ID.ToString()})</b>"));

#line default
#line hidden
            EndContext();
            BeginContext(1952, 31, true);
            WriteLiteral("</td>\r\n                    <td>");
            EndContext();
            BeginContext(1984, 23, false);
#line 43 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw(p.RequestPath));

#line default
#line hidden
            EndContext();
            BeginContext(2007, 33, true);
            WriteLiteral("</td>\r\n                    <td>\r\n");
            EndContext();
#line 45 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 if (SecurityFunctions.HasAccessTo(AdminPanelPages.EditProduct, user, Context))
                {

#line default
#line hidden
            BeginContext(2156, 107, true);
            WriteLiteral("                        <form method=\"get\">\r\n                            <input type=\"hidden\" name=\"pageID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2263, "\"", 2306, 1);
#line 48 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2271, (int)AdminPanelPages.EditProduct, 2271, 35, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2307, 67, true);
            WriteLiteral(" />\r\n                            <input type=\"hidden\" name=\"itemID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2374, "\"", 2387, 1);
#line 49 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2382, p.ID, 2382, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2388, 73, true);
            WriteLiteral(">\r\n                            <input type=\"submit\" class=\"action-button\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2461, "\"", 2504, 1);
#line 50 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2469, Html.Raw(localization.EditProduct), 2469, 35, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2505, 38, true);
            WriteLiteral(" />\r\n                        </form>\r\n");
            EndContext();
#line 52 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                }

#line default
#line hidden
            BeginContext(2562, 16, true);
            WriteLiteral("                ");
            EndContext();
#line 53 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 if (SecurityFunctions.HasAccessTo(AdminPanelPages.ProductImages, user, Context))
                {

#line default
#line hidden
            BeginContext(2680, 91, true);
            WriteLiteral("        <form method=\"get\">\r\n                            <input type=\"hidden\" name=\"pageID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2771, "\"", 2816, 1);
#line 56 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2779, (int)AdminPanelPages.ProductImages, 2779, 37, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2817, 67, true);
            WriteLiteral(" />\r\n                            <input type=\"hidden\" name=\"itemID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2884, "\"", 2897, 1);
#line 57 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2892, p.ID, 2892, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2898, 73, true);
            WriteLiteral(">\r\n                            <input type=\"submit\" class=\"action-button\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2971, "\"", 3016, 1);
#line 58 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2979, Html.Raw(localization.ProductImages), 2979, 37, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3017, 38, true);
            WriteLiteral(" />\r\n                        </form>\r\n");
            EndContext();
#line 60 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                }

#line default
#line hidden
            BeginContext(3074, 16, true);
            WriteLiteral("                ");
            EndContext();
#line 61 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 if (SecurityFunctions.HasAccessTo(AdminPanelPages.DeleteProduct, user, Context))
                {

#line default
#line hidden
            BeginContext(3192, 17, true);
            WriteLiteral("            <form");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3209, "\"", 3234, 2);
            WriteAttributeValue("", 3214, "delete-product-", 3214, 15, true);
#line 63 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 3229, p.ID, 3229, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3235, 65, true);
            WriteLiteral(">\r\n                            <input type=\"hidden\" name=\"pageID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 3300, "\"", 3345, 1);
#line 64 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 3308, (int)AdminPanelPages.DeleteProduct, 3308, 37, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3346, 65, true);
            WriteLiteral(">\r\n                            <input type=\"hidden\" name=\"itemID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 3411, "\"", 3424, 1);
#line 65 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 3419, p.ID, 3419, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3425, 37, true);
            WriteLiteral(">\r\n                            <input");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3462, "\"", 3494, 2);
            WriteAttributeValue("", 3467, "delete-product-button-", 3467, 22, true);
#line 66 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 3489, p.ID, 3489, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3495, 36, true);
            WriteLiteral(" type=\"submit\" class=\"action-button\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 3531, "\"", 3576, 1);
#line 66 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 3539, Html.Raw(localization.DeleteProduct), 3539, 37, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3577, 147, true);
            WriteLiteral(" />\r\n                        </form>\r\n                        <script>\r\n                            document.getElementById(\'delete-product-button-");
            EndContext();
            BeginContext(3725, 4, false);
#line 69 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                      Write(p.ID);

#line default
#line hidden
            EndContext();
            BeginContext(3729, 82, true);
            WriteLiteral("\').addEventListener(\'click\', createSendDataEventHandler(\'DELETE\', \'delete-product-");
            EndContext();
            BeginContext(3812, 4, false);
#line 69 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                                                                                                             Write(p.ID);

#line default
#line hidden
            EndContext();
            BeginContext(3816, 58, true);
            WriteLiteral("\', responseHandler));\r\n                        </script>\r\n");
            EndContext();
#line 71 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                }

#line default
#line hidden
            BeginContext(3893, 50, true);
            WriteLiteral("                    </td>\r\n                </tr>\r\n");
            EndContext();
#line 74 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
    }

#line default
#line hidden
            BeginContext(3950, 20, true);
            WriteLiteral("            </table>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public ICategoriesAndProductsLocalization localization { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IHostingEnvironment env { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<List<ProductPage>> Html { get; private set; }
    }
}
#pragma warning restore 1591
