#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2bcde253b481284d241ef6a0b963068c801de3a7"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_AdminPanel_CommonPage), @"mvc.1.0.view", @"/Views/AdminPanel/CommonPage.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/AdminPanel/CommonPage.cshtml", typeof(AspNetCore.Views_AdminPanel_CommonPage))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"2bcde253b481284d241ef6a0b963068c801de3a7", @"/Views/AdminPanel/CommonPage.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"643dee66794a2698b58719ead6ad3b58418e7a0e", @"/Views/AdminPanel/_ViewImports.cshtml")]
    public class Views_AdminPanel_CommonPage : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
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
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
  
    User user = Context.Items["User"] as User;
    AdminPanelPages pageID = (AdminPanelPages)Context.Items["pageID"];
    string pageName = Context.Items["PageName"] as string;

    Dictionary<AdminPanelPages, string> pages = new Dictionary<AdminPanelPages, string>();
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.MainPage, user, Context))
    {
        pages.Add(AdminPanelPages.MainPage, localization.MainPage);
    }
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.Pages, user, Context))
    {
        pages.Add(AdminPanelPages.Pages, localization.Pages);
    }
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.Categories, user, Context))
    {
        pages.Add(AdminPanelPages.Categories, localization.CategoriesAndProducts);
    }
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.Templates, user, Context))
    {
        pages.Add(AdminPanelPages.Templates, localization.Templates);
    }
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.Chunks, user, Context))
    {
        pages.Add(AdminPanelPages.Chunks, localization.Chunks);
    }
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.FileManager, user, Context))
    {
        pages.Add(AdminPanelPages.FileManager, localization.FileManager);
    }
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.SynonymsForStrings, user, Context))
    {
        pages.Add(AdminPanelPages.SynonymsForStrings, localization.SynonymsForStrings);
    }
    if (SecurityFunctions.HasAccessTo(AdminPanelPages.Settings, user, Context))
    {
        pages.Add(AdminPanelPages.Settings, localization.Settings);
    }

#line default
#line hidden
            BeginContext(1687, 25, true);
            WriteLiteral("<!doctype html>\r\n<html>\r\n");
            EndContext();
            BeginContext(1712, 540, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "980b1f610fc34566a5bff57324ec62ed", async() => {
                BeginContext(1718, 13, true);
                WriteLiteral("\r\n    <title>");
                EndContext();
                BeginContext(1732, 60, false);
#line 44 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
      Write(Html.Raw(pageName == null ? string.Empty : $"{pageName} - "));

#line default
#line hidden
                EndContext();
                BeginContext(1793, 28, false);
#line 44 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
                                                                   Write(Html.Raw(localization.Title));

#line default
#line hidden
                EndContext();
                BeginContext(1821, 215, true);
                WriteLiteral("</title>\r\n    <meta charset=\"utf-8\" />\r\n    <link rel=\"stylesheet\" type=\"text/css\" href=\"/styles/admin_panel/common.css\" />\r\n    <link rel=\"stylesheet\" type=\"text/css\" href=\"/styles/admin_panel/admin_panel.css\" />\r\n");
                EndContext();
#line 48 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
     if (Context.Items["PageStyle"] != null)
    {

#line default
#line hidden
                BeginContext(2089, 42, true);
                WriteLiteral("    <link rel=\"stylesheet\" type=\"text/css\"");
                EndContext();
                BeginWriteAttribute("href", " href=\"", 2131, "\"", 2175, 1);
#line 50 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
WriteAttributeValue("", 2138, Html.Raw(Context.Items["PageStyle"]), 2138, 37, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(2176, 5, true);
                WriteLiteral(" />\r\n");
                EndContext();
#line 51 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
    }

#line default
#line hidden
                BeginContext(2188, 57, true);
                WriteLiteral("    <meta name=\"viewport\" content=\"width=device-width\">\r\n");
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
            BeginContext(2252, 2, true);
            WriteLiteral("\r\n");
            EndContext();
            BeginContext(2254, 559, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "6749ef318bcc4165baecfa3633ef46ed", async() => {
                BeginContext(2260, 69, true);
                WriteLiteral("\r\n    <div class=\"commonContainer\">\r\n        <ul class=\"menuBlock\">\r\n");
                EndContext();
#line 57 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
             foreach (var p in pages)
            {
                if (pageID == p.Key)
                {

#line default
#line hidden
                BeginContext(2440, 36, true);
                WriteLiteral("            <li class=\"withPadding\">");
                EndContext();
                BeginContext(2477, 17, false);
#line 61 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
                               Write(Html.Raw(p.Value));

#line default
#line hidden
                EndContext();
                BeginContext(2494, 7, true);
                WriteLiteral("</li>\r\n");
                EndContext();
#line 62 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
                }
                else
                {

#line default
#line hidden
                BeginContext(2561, 18, true);
                WriteLiteral("            <li><a");
                EndContext();
                BeginWriteAttribute("href", " href=\"", 2579, "\"", 2628, 3);
#line 65 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
WriteAttributeValue("", 2586, Context.Request.Path, 2586, 21, false);

#line default
#line hidden
                WriteAttributeValue("", 2607, "?pageID=", 2607, 8, true);
#line 65 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
WriteAttributeValue("", 2615, (int)p.Key, 2615, 13, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(2629, 21, true);
                WriteLiteral(" class=\"withPadding\">");
                EndContext();
                BeginContext(2651, 17, false);
#line 65 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
                                                                                    Write(Html.Raw(p.Value));

#line default
#line hidden
                EndContext();
                BeginContext(2668, 11, true);
                WriteLiteral("</a></li>\r\n");
                EndContext();
#line 66 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
                }
            }

#line default
#line hidden
                BeginContext(2713, 46, true);
                WriteLiteral("        </ul>\r\n        <content>\r\n            ");
                EndContext();
                BeginContext(2760, 12, false);
#line 70 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\CommonPage.cshtml"
       Write(RenderBody());

#line default
#line hidden
                EndContext();
                BeginContext(2772, 34, true);
                WriteLiteral("\r\n        </content>\r\n    </div>\r\n");
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
            BeginContext(2813, 9, true);
            WriteLiteral("\r\n</html>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IAdminPanelPageLocalization localization { get; private set; }
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
