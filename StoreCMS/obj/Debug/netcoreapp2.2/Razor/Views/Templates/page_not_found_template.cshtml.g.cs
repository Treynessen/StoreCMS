#pragma checksum "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "533c2a4af1e9431caa7fb4a7ff6dd6a3688bf3b4"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Templates_page_not_found_template), @"mvc.1.0.view", @"/Views/Templates/page_not_found_template.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Templates/page_not_found_template.cshtml", typeof(AspNetCore.Views_Templates_page_not_found_template))]
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
#line 1 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Functions;

#line default
#line hidden
#line 2 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Controllers;

#line default
#line hidden
#line 3 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.PagesManagement;

#line default
#line hidden
#line 4 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.ImagesManagement;

#line default
#line hidden
#line 5 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 6 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"533c2a4af1e9431caa7fb4a7ff6dd6a3688bf3b4", @"/Views/Templates/page_not_found_template.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a171fe621900d30ce349d435054eba87062936fb", @"/Views/Templates/_ViewImports.cshtml")]
    public class Views_Templates_page_not_found_template : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Page>
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
            BeginContext(45, 798, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "533c2a4af1e9431caa7fb4a7ff6dd6a3688bf3b44182", async() => {
                BeginContext(51, 488, true);
                WriteLiteral(@"
<title>Ошибка 404 - магазин «Инструмент»</title> 
<link rel=""stylesheet"" href=""/styles/common.css"">
<link rel=""stylesheet"" href=""/styles/header.css"">
<link rel=""stylesheet"" href=""/styles/menu.css"">
<link rel=""stylesheet"" href=""/styles/content.css"">
<link rel=""stylesheet"" href=""/styles/page_not_found.css"">
<link rel=""stylesheet"" href=""/styles/footer.css"">
<link rel=""icon"" href=""/favicon.ico"">
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
<meta name=""description""");
                EndContext();
                BeginWriteAttribute("content", " content=\"", 539, "\"", 624, 1);
#line 14 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
WriteAttributeValue("", 549, Model != null ? Html.Raw(Model.PageDescription) : Html.Raw(string.Empty), 549, 75, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(625, 23, true);
                WriteLiteral(">\n<meta name=\"keywords\"");
                EndContext();
                BeginWriteAttribute("content", " content=\"", 648, "\"", 730, 1);
#line 15 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
WriteAttributeValue("", 658, Model != null ? Html.Raw(Model.PageKeywords) : Html.Raw(string.Empty), 658, 72, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(731, 105, true);
                WriteLiteral(">\n<meta name=\"robots\" content=\"[Page:IsRobotIndex]\">\n<meta name=\"viewport\" content=\"width=device-width\">\n");
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
            BeginContext(843, 1, true);
            WriteLiteral("\n");
            EndContext();
            BeginContext(844, 464, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "533c2a4af1e9431caa7fb4a7ff6dd6a3688bf3b47005", async() => {
                BeginContext(850, 1, true);
                WriteLiteral("\n");
                EndContext();
#line 20 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(861, 62, false);
#line 20 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/header.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 20 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
                                                                         } catch { } 

#line default
#line hidden
#line 21 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(948, 60, false);
#line 21 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/menu.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 21 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
                                                                       } catch { } 

#line default
#line hidden
#line 22 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(1033, 78, false);
#line 22 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/page_not_found_content.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 22 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
                                                                                         } catch { } 

#line default
#line hidden
#line 23 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(1136, 62, false);
#line 23 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/footer.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 23 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
                                                                         } catch { } 

#line default
#line hidden
#line 24 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
   try { 

#line default
#line hidden
                BeginContext(1223, 63, false);
#line 24 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
    Write(await Html.PartialAsync("~/Views/Chunks/scripts.cshtml", Model));

#line default
#line hidden
                EndContext();
#line 24 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Templates\page_not_found_template.cshtml"
                                                                          } catch { } 

#line default
#line hidden
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
            BeginContext(1308, 9, true);
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
