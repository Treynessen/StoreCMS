#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "017a73f441f106aa3dfd355b6da4db3c594325c1"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Chunks_category_page_content), @"mvc.1.0.view", @"/Views/Chunks/category_page_content.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Chunks/category_page_content.cshtml", typeof(AspNetCore.Views_Chunks_category_page_content))]
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
#line 1 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Functions;

#line default
#line hidden
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Controllers;

#line default
#line hidden
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.PagesManagement;

#line default
#line hidden
#line 4 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.ImagesManagement;

#line default
#line hidden
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"017a73f441f106aa3dfd355b6da4db3c594325c1", @"/Views/Chunks/category_page_content.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a171fe621900d30ce349d435054eba87062936fb", @"/Views/Chunks/_ViewImports.cshtml")]
    public class Views_Chunks_category_page_content : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Page>
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
        private global::Treynessen.TagHelpers.PageButtonsTagHelper __Treynessen_TagHelpers_PageButtonsTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
   List<ProductPage> products = Context.Items["products"] as List<ProductPage>; 

#line default
#line hidden
            BeginContext(94, 144, true);
            WriteLiteral("<div class=\"sort-panel\"><span>Сортировать цены: <b class=\"sort-menu\"></b></span></div>\n<div class=\"content-block\">\n<div class=\"navigation-menu\">");
            EndContext();
            BeginContext(240, 72, false);
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
                         Write(Model != null ? Html.Raw(Model.BreadcrumbsHtml) : Html.Raw(string.Empty));

#line default
#line hidden
            EndContext();
            BeginContext(313, 3, true);
            WriteLiteral(" → ");
            EndContext();
            BeginContext(318, 65, false);
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
                                                                                                       Write(Model != null ? Html.Raw(Model.PageName) : Html.Raw(string.Empty));

#line default
#line hidden
            EndContext();
            BeginContext(384, 7, true);
            WriteLiteral("</div>\n");
            EndContext();
            BeginContext(393, 64, false);
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
Write(Model != null ? Html.Raw(Model.Content) : Html.Raw(string.Empty));

#line default
#line hidden
            EndContext();
            BeginContext(458, 1, true);
            WriteLiteral("\n");
            EndContext();
#line 7 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
   if (products != null) { foreach (var p in products) { try { 

#line default
#line hidden
            BeginContext(523, 62, false);
#line 7 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
                                                          Write(await Html.PartialAsync(@"~/Settings/product_block.cshtml", p));

#line default
#line hidden
            EndContext();
#line 7 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
                                                                                                                              ; } catch { } } } 

#line default
#line hidden
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
   if (products != null) {

#line default
#line hidden
            BeginContext(631, 1, true);
            WriteLiteral(" ");
            EndContext();
            BeginContext(632, 220, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("page-buttons", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "89d5ffc4cae84b6fa81f1f28970b5b7b", async() => {
            }
            );
            __Treynessen_TagHelpers_PageButtonsTagHelper = CreateTagHelper<global::Treynessen.TagHelpers.PageButtonsTagHelper>();
            __tagHelperExecutionContext.Add(__Treynessen_TagHelpers_PageButtonsTagHelper);
            BeginWriteTagHelperAttribute();
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
                                    WriteLiteral(Context.Items["PaginationStyleName"]);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Treynessen_TagHelpers_PageButtonsTagHelper.Class = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("class", __Treynessen_TagHelpers_PageButtonsTagHelper.Class, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            BeginWriteTagHelperAttribute();
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
                                                                                         WriteLiteral(Context.Request.Path);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __Treynessen_TagHelpers_PageButtonsTagHelper.CurrentPath = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("current-path", __Treynessen_TagHelpers_PageButtonsTagHelper.CurrentPath, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
__Treynessen_TagHelpers_PageButtonsTagHelper.CurrentPage = (Context.Items["CurrentPage"] as int?);

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("current-page", __Treynessen_TagHelpers_PageButtonsTagHelper.CurrentPage, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
__Treynessen_TagHelpers_PageButtonsTagHelper.PagesCount = (Context.Items["PagesCount"] as int?);

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("pages-count", __Treynessen_TagHelpers_PageButtonsTagHelper.PagesCount, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(852, 1, true);
            WriteLiteral(" ");
            EndContext();
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\category_page_content.cshtml"
                                                                                                                                                                                                                                                        } 

#line default
#line hidden
            BeginContext(857, 7, true);
            WriteLiteral("</div>\n");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Page> Html { get; private set; }
    }
}
#pragma warning restore 1591
