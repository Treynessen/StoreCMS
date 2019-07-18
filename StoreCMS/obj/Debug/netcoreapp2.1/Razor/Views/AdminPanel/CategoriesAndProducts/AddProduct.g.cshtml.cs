#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b83c34880a1843d4708a1e7463c953788387e50d"
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b83c34880a1843d4708a1e7463c953788387e50d", @"/Views/AdminPanel/CategoriesAndProducts/AddProduct.cshtml")]
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
            BeginContext(436, 280, true);
            WriteLiteral(@"<script src=""/scripts/admin_panel/send_data.js""></script>
            <script src=""/scripts/admin_panel/insert_tab.js""></script>
            <script src=""/scripts/admin_panel/checkbox_event_handler.js""></script>
            <form id=""add-product-form"" class=""page-container"">
");
            EndContext();
            BeginContext(749, 60, true);
            WriteLiteral("                <input id=\"add-product-button\" type=\"submit\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 809, "\"", 851, 1);
#line 15 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
WriteAttributeValue("", 817, Html.Raw(localization.SaveButton), 817, 34, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(852, 89, true);
            WriteLiteral(" />\r\n                <div class=\"blocks\">\r\n                    <div class=\"left-block\">\r\n");
            EndContext();
            BeginContext(976, 43, true);
            WriteLiteral("                        <label for=\"Title\">");
            EndContext();
            BeginContext(1020, 35, false);
#line 19 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                      Write(Html.Raw(localization.ProductTitle));

#line default
#line hidden
            EndContext();
            BeginContext(1055, 101, true);
            WriteLiteral("*</label>\r\n                        <input type=\"text\" id=\"Title\" name=\"PageModel.Title\" required />\r\n");
            EndContext();
            BeginContext(1196, 46, true);
            WriteLiteral("                        <label for=\"PageName\">");
            EndContext();
            BeginContext(1243, 40, false);
#line 22 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                         Write(Html.Raw(localization.ProductBreadcrumb));

#line default
#line hidden
            EndContext();
            BeginContext(1283, 107, true);
            WriteLiteral("*</label>\r\n                        <input type=\"text\" id=\"PageName\" name=\"PageModel.PageName\" required />\r\n");
            EndContext();
            BeginContext(1425, 137, true);
            WriteLiteral("                        <div class=\"price-block\">\r\n                            <div>\r\n                                <label for=\"Price\">");
            EndContext();
            BeginContext(1563, 28, false);
#line 27 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                              Write(Html.Raw(localization.Price));

#line default
#line hidden
            EndContext();
            BeginContext(1591, 226, true);
            WriteLiteral("</label>\r\n                                <input type=\"number\" id=\"Price\" name=\"PageModel.Price\" />\r\n                            </div>\r\n                            <div>\r\n                                <label for=\"OldPrice\">");
            EndContext();
            BeginContext(1818, 31, false);
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                 Write(Html.Raw(localization.OldPrice));

#line default
#line hidden
            EndContext();
            BeginContext(1849, 175, true);
            WriteLiteral("</label>\r\n                                <input type=\"number\" id=\"OldPrice\" name=\"PageModel.OldPrice\" />\r\n                            </div>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(2071, 54, true);
            WriteLiteral("                        <label for=\"ShortDescription\">");
            EndContext();
            BeginContext(2126, 39, false);
#line 36 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                 Write(Html.Raw(localization.ShortDescription));

#line default
#line hidden
            EndContext();
            BeginContext(2165, 148, true);
            WriteLiteral("</label>\r\n                        <textarea class=\"short-description\" rows=\"3\" id=\"ShortDescription\" name=\"PageModel.ShortDescription\"></textarea>\r\n");
            EndContext();
            BeginContext(2362, 518, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""SpecialProduct"" name=""PageModel.SpecialProduct"" value=""false"" />
                            <script>
                                let specialProductCheckbox = document.getElementById('SpecialProduct');
                                specialProductCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""SpecialProduct"">");
            EndContext();
            BeginContext(2881, 37, false);
#line 45 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                   Write(Html.Raw(localization.SpecialProduct));

#line default
#line hidden
            EndContext();
            BeginContext(2918, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(3008, 524, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""AddParagraphTag"" name=""PageModel.AddParagraphTag"" value=""false"" />
                            <script>
                                let addParagraphTagCheckbox = document.getElementById('AddParagraphTag');
                                addParagraphTagCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""AddParagraphTag"">");
            EndContext();
            BeginContext(3533, 38, false);
#line 54 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                    Write(Html.Raw(localization.AddParagraphTag));

#line default
#line hidden
            EndContext();
            BeginContext(3571, 117, true);
            WriteLiteral("</label>\r\n                        </div>\r\n                    </div>\r\n                    <div class=\"right-block\">\r\n");
            EndContext();
            BeginContext(3732, 48, true);
            WriteLiteral("                        <label for=\"TemplateId\">");
            EndContext();
            BeginContext(3781, 31, false);
#line 59 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                           Write(Html.Raw(localization.Template));

#line default
#line hidden
            EndContext();
            BeginContext(3812, 133, true);
            WriteLiteral("</label>\r\n                        <select id=\"TemplateId\" name=\"PageModel.TemplateId\">\r\n                            <option selected>");
            EndContext();
            BeginContext(3946, 38, false);
#line 61 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                        Write(Html.Raw(localization.WithoutTemplate));

#line default
#line hidden
            EndContext();
            BeginContext(3984, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 62 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                             foreach (var t in templates)
                            {

#line default
#line hidden
            BeginContext(4085, 35, true);
            WriteLiteral("                            <option");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 4120, "\"", 4133, 1);
#line 64 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
WriteAttributeValue("", 4128, t.ID, 4128, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(4134, 1, true);
            WriteLiteral(">");
            EndContext();
            BeginContext(4136, 16, false);
#line 64 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                             Write(Html.Raw(t.Name));

#line default
#line hidden
            EndContext();
            BeginContext(4152, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 65 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                            }

#line default
#line hidden
            BeginContext(4194, 35, true);
            WriteLiteral("                        </select>\r\n");
            EndContext();
            BeginContext(4264, 43, true);
            WriteLiteral("                        <label for=\"Alias\">");
            EndContext();
            BeginContext(4308, 28, false);
#line 68 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                      Write(Html.Raw(localization.Alias));

#line default
#line hidden
            EndContext();
            BeginContext(4336, 91, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"Alias\" name=\"PageModel.Alias\" />\r\n");
            EndContext();
            BeginContext(4473, 53, true);
            WriteLiteral("                        <label for=\"PageDescription\">");
            EndContext();
            BeginContext(4527, 38, false);
#line 71 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                Write(Html.Raw(localization.PageDescription));

#line default
#line hidden
            EndContext();
            BeginContext(4565, 125, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"PageDescription\" name=\"PageModel.PageDescription\" maxlength=\"160\">\r\n");
            EndContext();
            BeginContext(4733, 50, true);
            WriteLiteral("                        <label for=\"PageKeywords\">");
            EndContext();
            BeginContext(4784, 35, false);
#line 74 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                             Write(Html.Raw(localization.PageKeywords));

#line default
#line hidden
            EndContext();
            BeginContext(4819, 105, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"PageKeywords\" name=\"PageModel.PageKeywords\" />\r\n");
            EndContext();
            BeginContext(4964, 495, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""Published"" name=""PageModel.Published"" value=""true"" checked />
                            <script>
                                let publishedCheckbox = document.getElementById('Published');
                                publishedCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""Published"">");
            EndContext();
            BeginContext(5460, 32, false);
#line 83 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                              Write(Html.Raw(localization.Published));

#line default
#line hidden
            EndContext();
            BeginContext(5492, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(5573, 483, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""IsIndex"" name=""PageModel.IsIndex"" value=""true"" checked />
                            <script>
                                let isIndexCheckbox = document.getElementById('IsIndex');
                                isIndexCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""IsIndex"">");
            EndContext();
            BeginContext(6057, 30, false);
#line 92 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                            Write(Html.Raw(localization.IsIndex));

#line default
#line hidden
            EndContext();
            BeginContext(6087, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(6169, 489, true);
            WriteLiteral(@"                        <div class=""checkbox-block"">
                            <input type=""checkbox"" id=""IsFollow"" name=""PageModel.IsFollow"" value=""true"" checked />
                            <script>
                                let isFollowCheckbox = document.getElementById('IsFollow');
                                isFollowCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""IsFollow"">");
            EndContext();
            BeginContext(6659, 31, false);
#line 101 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                             Write(Html.Raw(localization.IsFollow));

#line default
#line hidden
            EndContext();
            BeginContext(6690, 94, true);
            WriteLiteral("</label>\r\n                        </div>\r\n                    </div>\r\n                </div>\r\n");
            EndContext();
            BeginContext(6813, 37, true);
            WriteLiteral("                <label for=\"Content\">");
            EndContext();
            BeginContext(6851, 30, false);
#line 106 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                Write(Html.Raw(localization.Content));

#line default
#line hidden
            EndContext();
            BeginContext(6881, 296, true);
            WriteLiteral(@"</label>
                <textarea id=""Content"" name=""PageModel.Content""></textarea>
            </form>
            <script>
                function errorHandler(formElement) {
                    let errorMsg = document.createElement('span');
                    errorMsg.textContent = '");
            EndContext();
            BeginContext(7178, 37, false);
#line 112 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                       Write(Html.Raw(localization.IncorrectInput));

#line default
#line hidden
            EndContext();
            BeginContext(7215, 256, true);
            WriteLiteral(@"';
                    errorMsg.setAttribute('id', 'error-msg');
                    formElement.insertBefore(errorMsg, formElement.firstChild);
                }
                function successfulRequestHandler(request) {
                    alert('");
            EndContext();
            BeginContext(7472, 35, false);
#line 117 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                      Write(Html.Raw(localization.ProductAdded));

#line default
#line hidden
            EndContext();
            BeginContext(7507, 146, true);
            WriteLiteral("\');\r\n                    location.replace(request.getResponseHeader(\'location\'));\r\n                }\r\n                let searchString = \'?pageID=");
            EndContext();
            BeginContext(7655, 31, false);
#line 120 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                        Write((int)AdminPanelPages.AddProduct);

#line default
#line hidden
            EndContext();
            BeginContext(7687, 8, true);
            WriteLiteral("&itemID=");
            EndContext();
            BeginContext(7696, 16, false);
#line 120 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CategoriesAndProducts\AddProduct.cshtml"
                                                                                 Write(categoryPage?.ID);

#line default
#line hidden
            EndContext();
            BeginContext(7712, 630, true);
            WriteLiteral(@"';
                let addProductButton = document.getElementById('add-product-button');
                addProductButton.addEventListener('click', createSendDataEventHandler('POST', searchString, 'add-product-form', successfulRequestHandler, errorHandler));
                let textareaShortDescription = document.getElementById('ShortDescription');
                textareaShortDescription.addEventListener('keydown', insertTabEventHandler);
                let textareaContent = document.getElementById('Content');
                textareaContent.addEventListener('keydown', insertTabEventHandler);
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
