#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "c9910c79b0714ba22ae5bfa8f460e2658b8d8db4"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Templates_category_page_template), @"mvc.1.0.view", @"/Views/Templates/category_page_template.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Templates/category_page_template.cshtml", typeof(AspNetCore.Views_Templates_category_page_template))]
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
#line 1 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Functions;

#line default
#line hidden
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Controllers;

#line default
#line hidden
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.PagesManagement;

#line default
#line hidden
#line 4 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.ImagesManagement;

#line default
#line hidden
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c9910c79b0714ba22ae5bfa8f460e2658b8d8db4", @"/Views/Templates/category_page_template.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a171fe621900d30ce349d435054eba87062936fb", @"/Views/Templates/_ViewImports.cshtml")]
    public class Views_Templates_category_page_template : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Page>
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
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper;
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(12, 33, true);
            WriteLiteral("<!DOCTYPE html>\n<html lang=\"ru\">\n");
            EndContext();
            BeginContext(45, 772, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "e7cf3b19dce84eaaa669764d387cea4f", async() => {
                BeginContext(51, 8, true);
                WriteLiteral("\n<title>");
                EndContext();
                BeginContext(61, 62, false);
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
   Write(Model != null ? Html.Raw(Model.Title) : Html.Raw(string.Empty));

#line default
#line hidden
                EndContext();
                BeginContext(124, 441, true);
                WriteLiteral(@"</title> 
<link rel=""stylesheet"" href=""/styles/common.css"">
<link rel=""stylesheet"" href=""/styles/header.css"">
<link rel=""stylesheet"" href=""/styles/menu.css"">
<link rel=""stylesheet"" href=""/styles/content.css"">
<link rel=""stylesheet"" href=""/styles/category.css"">
<link rel=""stylesheet"" href=""/styles/footer.css"">
<link rel=""icon"" href=""/favicon.ico"">
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
<meta name=""description""");
                EndContext();
                BeginWriteAttribute("content", " content=\"", 565, "\"", 650, 1);
#line 14 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
WriteAttributeValue("", 575, Model != null ? Html.Raw(Model.PageDescription) : Html.Raw(string.Empty), 575, 75, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(651, 23, true);
                WriteLiteral(">\n<meta name=\"keywords\"");
                EndContext();
                BeginWriteAttribute("content", " content=\"", 674, "\"", 756, 1);
#line 15 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
WriteAttributeValue("", 684, Model != null ? Html.Raw(Model.PageKeywords) : Html.Raw(string.Empty), 684, 72, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(757, 53, true);
                WriteLiteral(">\n<meta name=\"robots\" content=\"[Page:IsRobotIndex]\">\n");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_HeadTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(817, 1, true);
            WriteLiteral("\n");
            EndContext();
            BeginContext(818, 510, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "1e62194e27724672959b00a14fdb7619", async() => {
                BeginContext(824, 1, true);
                WriteLiteral("\n");
                EndContext();
#line 19 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(835, 62, false);
#line 19 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/header.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 19 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
                                                                         } catch { } 

#line default
#line hidden
#line 20 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(922, 60, false);
#line 20 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/menu.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 20 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
                                                                       } catch { } 

#line default
#line hidden
#line 21 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(1007, 77, false);
#line 21 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/category_page_content.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 21 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
                                                                                        } catch { } 

#line default
#line hidden
#line 22 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(1109, 62, false);
#line 22 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/footer.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 22 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
                                                                         } catch { } 

#line default
#line hidden
#line 23 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(1196, 63, false);
#line 23 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/scripts.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 23 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\category_page_template.cshtml"
                                                                          } catch { } 

#line default
#line hidden
                BeginContext(1274, 47, true);
                WriteLiteral("<script src=\"/scripts/price_sort.js\"></script>\n");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_BodyTagHelper);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(1328, 9, true);
            WriteLiteral("\n</html>\n");
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
