#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b5bdf4ae43cbe6f8a365bf03b7ded057345dc446"
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
#line 1 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Security;

#line default
#line hidden
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Localization;

#line default
#line hidden
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.PagesManagement;

#line default
#line hidden
#line 4 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.AdminPanelTypes;

#line default
#line hidden
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
#line 1 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
using Microsoft.AspNetCore.Hosting;

#line default
#line hidden
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
using Treynessen.Extensions;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b5bdf4ae43cbe6f8a365bf03b7ded057345dc446", @"/Views/AdminPanel/CategoriesAndProducts/CategoryProducts.cshtml")]
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
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
  
    Layout = "CommonPage";
    Context.Items["PageStyle"] = "/styles/admin_panel/categories_and_products/category_products.css";
    int categoryID = (int)Context.Items["itemID"];
    string categoryName = Context.Items["categoryName"] as string;
    Context.Items["PageName"] = $"{localization.CategoryProductsPageName} {categoryName}";
    User user = Context.Items["User"] as User;

#line default
#line hidden
            BeginContext(583, 59, true);
            WriteLiteral("<script src=\"/scripts/admin_panel/send_data.js\"></script>\r\n");
            EndContext();
#line 15 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
 if (SecurityFunctions.HasAccessTo(AdminPanelPages.AddProduct, user, Context))
{

#line default
#line hidden
            BeginContext(725, 52, true);
            WriteLiteral("<div class=\"add-product-button\">\r\n                <a");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 777, "\"", 866, 5);
#line 18 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 784, Context.Request.Path, 784, 21, false);

#line default
#line hidden
            WriteAttributeValue("", 805, "?pageID=", 805, 8, true);
#line 18 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 813, (int)AdminPanelPages.AddProduct, 813, 34, false);

#line default
#line hidden
            WriteAttributeValue("", 847, "&itemID=", 847, 8, true);
#line 18 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 855, categoryID, 855, 11, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(867, 1, true);
            WriteLiteral(">");
            EndContext();
            BeginContext(869, 70, false);
#line 18 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                                                        Write(Html.Raw($"{localization.AddProductInCategory} <b>{categoryName}</b>"));

#line default
#line hidden
            EndContext();
            BeginContext(939, 26, true);
            WriteLiteral("</a>\r\n            </div>\r\n");
            EndContext();
#line 20 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
}

#line default
#line hidden
            BeginContext(968, 98, true);
            WriteLiteral("            <table>\r\n                <tr>\r\n                    <td></td>\r\n                    <td>");
            EndContext();
            BeginContext(1067, 27, false);
#line 24 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw(localization.Name));

#line default
#line hidden
            EndContext();
            BeginContext(1094, 65, true);
            WriteLiteral("</td>\r\n                    <td>URL</td>\r\n                    <td>");
            EndContext();
            BeginContext(1160, 30, false);
#line 26 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw(localization.Actions));

#line default
#line hidden
            EndContext();
            BeginContext(1190, 30, true);
            WriteLiteral("</td>\r\n                </tr>\r\n");
            EndContext();
#line 28 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 foreach (var p in Model)
                {

#line default
#line hidden
            BeginContext(1282, 46, true);
            WriteLiteral("                <tr>\r\n                    <td>");
            EndContext();
            BeginContext(1328, 176, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("image", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "d909f065534f490680815876280f9a89", async() => {
            }
            );
            __Treynessen_TagHelpers_ImageTagHelper = CreateTagHelper<global::Treynessen.TagHelpers.ImageTagHelper>();
            __tagHelperExecutionContext.Add(__Treynessen_TagHelpers_ImageTagHelper);
            BeginWriteTagHelperAttribute();
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                        WriteLiteral(Html.Raw($"{env.GetProductsImagesFolderSrc()}{p.PreviousPageID.ToString()}{p.ID.ToString()}/{p.Alias}.jpg"));

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Treynessen_TagHelpers_ImageTagHelper.Src = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("src", __Treynessen_TagHelpers_ImageTagHelper.Src, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
__Treynessen_TagHelpers_ImageTagHelper.MaxHeight = 150;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("max-height", __Treynessen_TagHelpers_ImageTagHelper.MaxHeight, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
__Treynessen_TagHelpers_ImageTagHelper.MaxWidth = 150;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("max-width", __Treynessen_TagHelpers_ImageTagHelper.MaxWidth, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
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
            BeginContext(1504, 31, true);
            WriteLiteral("</td>\r\n                    <td>");
            EndContext();
            BeginContext(1536, 54, false);
#line 32 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw($"{p.PageName} <b>(ID{p.ID.ToString()})</b>"));

#line default
#line hidden
            EndContext();
            BeginContext(1590, 31, true);
            WriteLiteral("</td>\r\n                    <td>");
            EndContext();
            BeginContext(1622, 23, false);
#line 33 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                   Write(Html.Raw(p.RequestPath));

#line default
#line hidden
            EndContext();
            BeginContext(1645, 33, true);
            WriteLiteral("</td>\r\n                    <td>\r\n");
            EndContext();
#line 35 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 if (SecurityFunctions.HasAccessTo(AdminPanelPages.EditProduct, user, Context))
                {

#line default
#line hidden
            BeginContext(1794, 107, true);
            WriteLiteral("                        <form method=\"get\">\r\n                            <input type=\"hidden\" name=\"pageID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 1901, "\"", 1944, 1);
#line 38 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 1909, (int)AdminPanelPages.EditProduct, 1909, 35, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1945, 67, true);
            WriteLiteral(" />\r\n                            <input type=\"hidden\" name=\"itemID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2012, "\"", 2025, 1);
#line 39 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2020, p.ID, 2020, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2026, 73, true);
            WriteLiteral(">\r\n                            <input type=\"submit\" class=\"action-button\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2099, "\"", 2142, 1);
#line 40 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2107, Html.Raw(localization.EditProduct), 2107, 35, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2143, 38, true);
            WriteLiteral(" />\r\n                        </form>\r\n");
            EndContext();
#line 42 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                }

#line default
#line hidden
            BeginContext(2200, 16, true);
            WriteLiteral("                ");
            EndContext();
#line 43 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 if (SecurityFunctions.HasAccessTo(AdminPanelPages.ProductImages, user, Context))
                {

#line default
#line hidden
            BeginContext(2318, 107, true);
            WriteLiteral("                        <form method=\"get\">\r\n                            <input type=\"hidden\" name=\"pageID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2425, "\"", 2470, 1);
#line 46 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2433, (int)AdminPanelPages.ProductImages, 2433, 37, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2471, 67, true);
            WriteLiteral(" />\r\n                            <input type=\"hidden\" name=\"itemID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2538, "\"", 2551, 1);
#line 47 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2546, p.ID, 2546, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2552, 73, true);
            WriteLiteral(">\r\n                            <input type=\"submit\" class=\"action-button\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2625, "\"", 2670, 1);
#line 48 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2633, Html.Raw(localization.ProductImages), 2633, 37, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2671, 38, true);
            WriteLiteral(" />\r\n                        </form>\r\n");
            EndContext();
#line 50 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                }

#line default
#line hidden
            BeginContext(2728, 16, true);
            WriteLiteral("                ");
            EndContext();
#line 51 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                 if (SecurityFunctions.HasAccessTo(AdminPanelPages.DeleteProduct, user, Context))
                {

#line default
#line hidden
            BeginContext(2846, 17, true);
            WriteLiteral("            <form");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 2863, "\"", 2893, 2);
            WriteAttributeValue("", 2868, "delete-product-form-", 2868, 20, true);
#line 53 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2888, p.ID, 2888, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2894, 65, true);
            WriteLiteral(">\r\n                            <input type=\"hidden\" name=\"itemID\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2959, "\"", 2972, 1);
#line 54 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 2967, p.ID, 2967, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2973, 37, true);
            WriteLiteral(">\r\n                            <input");
            EndContext();
            BeginWriteAttribute("id", " id=\"", 3010, "\"", 3042, 2);
            WriteAttributeValue("", 3015, "delete-product-button-", 3015, 22, true);
#line 55 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 3037, p.ID, 3037, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3043, 36, true);
            WriteLiteral(" type=\"submit\" class=\"action-button\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 3079, "\"", 3124, 1);
#line 55 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
WriteAttributeValue("", 3087, Html.Raw(localization.DeleteProduct), 3087, 37, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3125, 124, true);
            WriteLiteral(" />\r\n                        </form>\r\n                        <script>\r\n                            let deleteProductButton_");
            EndContext();
            BeginContext(3250, 4, false);
#line 58 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                               Write(p.ID);

#line default
#line hidden
            EndContext();
            BeginContext(3254, 50, true);
            WriteLiteral(" = document.getElementById(\'delete-product-button-");
            EndContext();
            BeginContext(3305, 4, false);
#line 58 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                                                      Write(p.ID);

#line default
#line hidden
            EndContext();
            BeginContext(3309, 53, true);
            WriteLiteral("\');\r\n                            deleteProductButton_");
            EndContext();
            BeginContext(3364, 4, false);
#line 59 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                            Write(p.ID);

#line default
#line hidden
            EndContext();
            BeginContext(3369, 73, true);
            WriteLiteral(".addEventListener(\'click\', createSendDataEventHandler(\'DELETE\', \'?pageID=");
            EndContext();
            BeginContext(3444, 34, false);
#line 59 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                                                                            Write((int)AdminPanelPages.DeleteProduct);

#line default
#line hidden
            EndContext();
            BeginContext(3479, 24, true);
            WriteLiteral("\', \'delete-product-form-");
            EndContext();
            BeginContext(3504, 4, false);
#line 59 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                                                                                                                                        Write(p.ID);

#line default
#line hidden
            EndContext();
            BeginContext(3508, 18, true);
            WriteLiteral("\', () => { alert(\'");
            EndContext();
            BeginContext(3527, 37, false);
#line 59 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                                                                                                                                                                                                               Write(Html.Raw(localization.ProductDeleted));

#line default
#line hidden
            EndContext();
            BeginContext(3564, 63, true);
            WriteLiteral("\'); location.reload(); }))\r\n                        </script>\r\n");
            EndContext();
#line 61 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
                }

#line default
#line hidden
            BeginContext(3646, 50, true);
            WriteLiteral("                    </td>\r\n                </tr>\r\n");
            EndContext();
#line 64 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\CategoryProducts.cshtml"
    }

#line default
#line hidden
            BeginContext(3703, 20, true);
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
