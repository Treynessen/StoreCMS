#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "13e34004767961cd454182c99be09b9b012b2113"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_AdminPanel_CategoriesAndProducts_AddProduct), @"mvc.1.0.view", @"/Views/AdminPanel/CategoriesAndProducts/AddProduct.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/AdminPanel/CategoriesAndProducts/AddProduct.cshtml", typeof(AspNetCore.Views_AdminPanel_CategoriesAndProducts_AddProduct))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"13e34004767961cd454182c99be09b9b012b2113", @"/Views/AdminPanel/CategoriesAndProducts/AddProduct.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"643dee66794a2698b58719ead6ad3b58418e7a0e", @"/Views/AdminPanel/_ViewImports.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"cd484aca00b6366db4c9613599bf6cd4bf9f84d6", @"/Views/AdminPanel/CategoriesAndProducts/_ViewImports.cshtml")]
    public class Views_AdminPanel_CategoriesAndProducts_AddProduct : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
  
    Layout = "CommonPage";
    Context.Items["PageStyle"] = "/styles/admin_panel/categories_and_products/work_with_page.css";
    CategoryPage categoryPage = Context.Items["CategoryPage"] as CategoryPage;
    Context.Items["PageName"] = $"{localization.AddProductPageName} {categoryPage?.PageName}";
    var templates = db.Templates.ToList();

#line default
#line hidden
            BeginContext(436, 208, true);
            WriteLiteral("<script src=\"/scripts/admin_panel/send_data.js\"></script>\r\n            <script src=\"/scripts/admin_panel/checkbox_event_handler.js\"></script>\r\n            <form id=\"add-product-form\" class=\"page-container\">\r\n");
            EndContext();
            BeginContext(677, 60, true);
            WriteLiteral("                <input id=\"add-product-button\" type=\"submit\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 737, "\"", 779, 1);
#line 14 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
WriteAttributeValue("", 745, Html.Raw(localization.SaveButton), 745, 34, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(780, 89, true);
            WriteLiteral(" />\r\n                <div class=\"blocks\">\r\n                    <div class=\"left-block\">\r\n");
            EndContext();
            BeginContext(904, 43, true);
            WriteLiteral("                        <label for=\"Title\">");
            EndContext();
            BeginContext(948, 35, false);
#line 18 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                      Write(Html.Raw(localization.ProductTitle));

#line default
#line hidden
            EndContext();
            BeginContext(983, 101, true);
            WriteLiteral("*</label>\r\n                        <input type=\"text\" id=\"Title\" name=\"PageModel.Title\" required />\r\n");
            EndContext();
            BeginContext(1124, 46, true);
            WriteLiteral("                        <label for=\"PageName\">");
            EndContext();
            BeginContext(1171, 40, false);
#line 21 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                         Write(Html.Raw(localization.ProductBreadcrumb));

#line default
#line hidden
            EndContext();
            BeginContext(1211, 107, true);
            WriteLiteral("*</label>\r\n                        <input type=\"text\" id=\"PageName\" name=\"PageModel.PageName\" required />\r\n");
            EndContext();
            BeginContext(1353, 137, true);
            WriteLiteral("                        <div class=\"price-block\">\r\n                            <div>\r\n                                <label for=\"Price\">");
            EndContext();
            BeginContext(1491, 28, false);
#line 26 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                              Write(Html.Raw(localization.Price));

#line default
#line hidden
            EndContext();
            BeginContext(1519, 226, true);
            WriteLiteral("</label>\r\n                                <input type=\"number\" id=\"Price\" name=\"PageModel.Price\" />\r\n                            </div>\r\n                            <div>\r\n                                <label for=\"OldPrice\">");
            EndContext();
            BeginContext(1746, 31, false);
#line 30 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                 Write(Html.Raw(localization.OldPrice));

#line default
#line hidden
            EndContext();
            BeginContext(1777, 175, true);
            WriteLiteral("</label>\r\n                                <input type=\"number\" id=\"OldPrice\" name=\"PageModel.OldPrice\" />\r\n                            </div>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(1999, 54, true);
            WriteLiteral("                        <label for=\"ShortDescription\">");
            EndContext();
            BeginContext(2054, 39, false);
#line 35 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                 Write(Html.Raw(localization.ShortDescription));

#line default
#line hidden
            EndContext();
            BeginContext(2093, 148, true);
            WriteLiteral("</label>\r\n                        <textarea class=\"short-description\" rows=\"3\" id=\"ShortDescription\" name=\"PageModel.ShortDescription\"></textarea>\r\n");
            EndContext();
            BeginContext(2290, 518, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""SpecialProduct"" name=""PageModel.SpecialProduct"" value=""false"" />
                            <script>
                                let specialProductCheckbox = document.getElementById('SpecialProduct');
                                specialProductCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""SpecialProduct"">");
            EndContext();
            BeginContext(2809, 37, false);
#line 44 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                   Write(Html.Raw(localization.SpecialProduct));

#line default
#line hidden
            EndContext();
            BeginContext(2846, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(2936, 524, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""AddParagraphTag"" name=""PageModel.AddParagraphTag"" value=""false"" />
                            <script>
                                let addParagraphTagCheckbox = document.getElementById('AddParagraphTag');
                                addParagraphTagCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""AddParagraphTag"">");
            EndContext();
            BeginContext(3461, 38, false);
#line 53 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                    Write(Html.Raw(localization.AddParagraphTag));

#line default
#line hidden
            EndContext();
            BeginContext(3499, 117, true);
            WriteLiteral("</label>\r\n                        </div>\r\n                    </div>\r\n                    <div class=\"right-block\">\r\n");
            EndContext();
            BeginContext(3660, 48, true);
            WriteLiteral("                        <label for=\"TemplateId\">");
            EndContext();
            BeginContext(3709, 31, false);
#line 58 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                           Write(Html.Raw(localization.Template));

#line default
#line hidden
            EndContext();
            BeginContext(3740, 133, true);
            WriteLiteral("</label>\r\n                        <select id=\"TemplateId\" name=\"PageModel.TemplateId\">\r\n                            <option selected>");
            EndContext();
            BeginContext(3874, 38, false);
#line 60 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                        Write(Html.Raw(localization.WithoutTemplate));

#line default
#line hidden
            EndContext();
            BeginContext(3912, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 61 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                             foreach (var t in templates)
                            {

#line default
#line hidden
            BeginContext(4013, 35, true);
            WriteLiteral("                            <option");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 4048, "\"", 4061, 1);
#line 63 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
WriteAttributeValue("", 4056, t.ID, 4056, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(4062, 1, true);
            WriteLiteral(">");
            EndContext();
            BeginContext(4064, 16, false);
#line 63 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                             Write(Html.Raw(t.Name));

#line default
#line hidden
            EndContext();
            BeginContext(4080, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 64 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                            }

#line default
#line hidden
            BeginContext(4122, 35, true);
            WriteLiteral("                        </select>\r\n");
            EndContext();
            BeginContext(4192, 43, true);
            WriteLiteral("                        <label for=\"Alias\">");
            EndContext();
            BeginContext(4236, 28, false);
#line 67 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                      Write(Html.Raw(localization.Alias));

#line default
#line hidden
            EndContext();
            BeginContext(4264, 91, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"Alias\" name=\"PageModel.Alias\" />\r\n");
            EndContext();
            BeginContext(4401, 53, true);
            WriteLiteral("                        <label for=\"PageDescription\">");
            EndContext();
            BeginContext(4455, 38, false);
#line 70 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                Write(Html.Raw(localization.PageDescription));

#line default
#line hidden
            EndContext();
            BeginContext(4493, 125, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"PageDescription\" name=\"PageModel.PageDescription\" maxlength=\"160\">\r\n");
            EndContext();
            BeginContext(4661, 50, true);
            WriteLiteral("                        <label for=\"PageKeywords\">");
            EndContext();
            BeginContext(4712, 35, false);
#line 73 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                             Write(Html.Raw(localization.PageKeywords));

#line default
#line hidden
            EndContext();
            BeginContext(4747, 105, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"PageKeywords\" name=\"PageModel.PageKeywords\" />\r\n");
            EndContext();
            BeginContext(4892, 495, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""Published"" name=""PageModel.Published"" value=""true"" checked />
                            <script>
                                let publishedCheckbox = document.getElementById('Published');
                                publishedCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""Published"">");
            EndContext();
            BeginContext(5388, 32, false);
#line 82 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                              Write(Html.Raw(localization.Published));

#line default
#line hidden
            EndContext();
            BeginContext(5420, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(5501, 483, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""IsIndex"" name=""PageModel.IsIndex"" value=""true"" checked />
                            <script>
                                let isIndexCheckbox = document.getElementById('IsIndex');
                                isIndexCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""IsIndex"">");
            EndContext();
            BeginContext(5985, 30, false);
#line 91 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                            Write(Html.Raw(localization.IsIndex));

#line default
#line hidden
            EndContext();
            BeginContext(6015, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(6097, 489, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""IsFollow"" name=""PageModel.IsFollow"" value=""true"" checked />
                            <script>
                                let isFollowCheckbox = document.getElementById('IsFollow');
                                isFollowCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""IsFollow"">");
            EndContext();
            BeginContext(6587, 31, false);
#line 100 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                             Write(Html.Raw(localization.IsFollow));

#line default
#line hidden
            EndContext();
            BeginContext(6618, 94, true);
            WriteLiteral("</label>\r\n                        </div>\r\n                    </div>\r\n                </div>\r\n");
            EndContext();
            BeginContext(6741, 37, true);
            WriteLiteral("                <label for=\"Content\">");
            EndContext();
            BeginContext(6779, 30, false);
#line 105 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                Write(Html.Raw(localization.Content));

#line default
#line hidden
            EndContext();
            BeginContext(6809, 296, true);
            WriteLiteral(@"</label>
                <textarea id=""Content"" name=""PageModel.Content""></textarea>
            </form>
            <script>
                function errorHandler(formElement) {
                    let errorMsg = document.createElement('span');
                    errorMsg.textContent = '");
            EndContext();
            BeginContext(7106, 37, false);
#line 111 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                       Write(Html.Raw(localization.IncorrectInput));

#line default
#line hidden
            EndContext();
            BeginContext(7143, 256, true);
            WriteLiteral(@"';
                    errorMsg.setAttribute('id', 'error-msg');
                    formElement.insertBefore(errorMsg, formElement.firstChild);
                }
                function successfulRequestHandler(request) {
                    alert('");
            EndContext();
            BeginContext(7400, 35, false);
#line 116 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                      Write(Html.Raw(localization.ProductAdded));

#line default
#line hidden
            EndContext();
            BeginContext(7435, 146, true);
            WriteLiteral("\');\r\n                    location.replace(request.getResponseHeader(\'location\'));\r\n                }\r\n                let searchString = \'?pageID=");
            EndContext();
            BeginContext(7583, 31, false);
#line 119 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                        Write((int)AdminPanelPages.AddProduct);

#line default
#line hidden
            EndContext();
            BeginContext(7615, 8, true);
            WriteLiteral("&itemID=");
            EndContext();
            BeginContext(7624, 16, false);
#line 119 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                                                 Write(categoryPage?.ID);

#line default
#line hidden
            EndContext();
            BeginContext(7640, 283, true);
            WriteLiteral(@"';
                let addProductButton = document.getElementById('add-product-button');
                addProductButton.addEventListener('click', createSendDataEventHandler('POST', searchString, 'add-product-form', successfulRequestHandler, errorHandler));
            </script>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public CMSDatabase db { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public ICategoriesAndProductsLocalization localization { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
