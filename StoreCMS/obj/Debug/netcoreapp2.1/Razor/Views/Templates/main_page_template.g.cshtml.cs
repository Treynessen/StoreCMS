#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "25e5c42eae777955761d1cd1e8a7d9bc8e175cdd"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Templates_main_page_template), @"mvc.1.0.view", @"/Views/Templates/main_page_template.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Templates/main_page_template.cshtml", typeof(AspNetCore.Views_Templates_main_page_template))]
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
using Treynessen.PagesManagement;

#line default
#line hidden
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"25e5c42eae777955761d1cd1e8a7d9bc8e175cdd", @"/Views/Templates/main_page_template.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0f171c8736a056a2cb5053592ce15da782087f9b", @"/Views/Templates/_ViewImports.cshtml")]
    public class Views_Templates_main_page_template : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Page>
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
            BeginContext(12, 35, true);
            WriteLiteral("<!DOCTYPE html>\r\n<html lang=\"ru\">\r\n");
            EndContext();
            BeginContext(47, 844, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "f03d92bf38cf4b9fab7aa63be2fa37d0", async() => {
                BeginContext(53, 9, true);
                WriteLiteral("\r\n<title>");
                EndContext();
                BeginContext(64, 62, false);
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
   Write(Model != null ? Html.Raw(Model.Title) : Html.Raw(string.Empty));

#line default
#line hidden
                EndContext();
                BeginContext(127, 509, true);
                WriteLiteral(@"</title> 
<link rel=""stylesheet"" href=""/styles/common.css"">
<link rel=""stylesheet"" href=""/styles/header.css"">
<link rel=""stylesheet"" href=""/styles/menu.css"">
<link rel=""stylesheet"" href=""/styles/notice.css"">
<link rel=""stylesheet"" href=""/styles/content.css"">
<link rel=""stylesheet"" href=""/styles/main_page_content.css"">
<link rel=""stylesheet"" href=""/styles/footer.css"">
<link rel=""icon"" href=""favicon.ico"">
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
<meta name=""description""");
                EndContext();
                BeginWriteAttribute("content", " content=\"", 636, "\"", 721, 1);
#line 15 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
WriteAttributeValue("", 646, Model != null ? Html.Raw(Model.PageDescription) : Html.Raw(string.Empty), 646, 75, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(722, 24, true);
                WriteLiteral(">\r\n<meta name=\"keywords\"");
                EndContext();
                BeginWriteAttribute("content", " content=\"", 746, "\"", 828, 1);
#line 16 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
WriteAttributeValue("", 756, Model != null ? Html.Raw(Model.PageKeywords) : Html.Raw(string.Empty), 756, 72, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(829, 55, true);
                WriteLiteral(">\r\n<meta name=\"robots\" content=\"[Page:IsRobotIndex]\">\r\n");
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
            BeginContext(891, 2, true);
            WriteLiteral("\r\n");
            EndContext();
            BeginContext(893, 545, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "afb05eef82204597928565af38f40b46", async() => {
                BeginContext(899, 2, true);
                WriteLiteral("\r\n");
                EndContext();
                BeginContext(903, 62, false);
#line 20 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
Write(await Html.PartialAsync("~/Views/Chunks/header.cshtml", Model));

#line default
#line hidden
                EndContext();
                BeginContext(966, 2, true);
                WriteLiteral("\r\n");
                EndContext();
                BeginContext(970, 60, false);
#line 21 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
Write(await Html.PartialAsync("~/Views/Chunks/menu.cshtml", Model));

#line default
#line hidden
                EndContext();
                BeginContext(1031, 198, true);
                WriteLiteral("\r\n<script>\r\n\tvar catalog = document.getElementById(\'catalog\');\r\n\tvar button = catalog.querySelector(\'.button\');\r\n\tbutton.onclick = function() {\r\n\t\tcatalog.classList.toggle(\'open\');\r\n\t};\r\n</script>\r\n");
                EndContext();
                BeginContext(1231, 62, false);
#line 29 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
Write(await Html.PartialAsync("~/Views/Chunks/notice.cshtml", Model));

#line default
#line hidden
                EndContext();
                BeginContext(1294, 2, true);
                WriteLiteral("\r\n");
                EndContext();
                BeginContext(1298, 63, false);
#line 30 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
Write(await Html.PartialAsync("~/Views/Chunks/content.cshtml", Model));

#line default
#line hidden
                EndContext();
                BeginContext(1362, 2, true);
                WriteLiteral("\r\n");
                EndContext();
                BeginContext(1366, 62, false);
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Templates\main_page_template.cshtml"
Write(await Html.PartialAsync("~/Views/Chunks/footer.cshtml", Model));

#line default
#line hidden
                EndContext();
                BeginContext(1429, 2, true);
                WriteLiteral("\r\n");
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
            BeginContext(1438, 9, true);
            WriteLiteral("\r\n</html>");
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
