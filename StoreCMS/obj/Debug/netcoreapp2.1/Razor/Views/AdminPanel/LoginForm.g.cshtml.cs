#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "04712cf71e836d4684dc939d56bb618780481afb"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_AdminPanel_LoginForm), @"mvc.1.0.view", @"/Views/AdminPanel/LoginForm.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/AdminPanel/LoginForm.cshtml", typeof(AspNetCore.Views_AdminPanel_LoginForm))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"04712cf71e836d4684dc939d56bb618780481afb", @"/Views/AdminPanel/LoginForm.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"643dee66794a2698b58719ead6ad3b58418e7a0e", @"/Views/AdminPanel/_ViewImports.cshtml")]
    public class Views_AdminPanel_LoginForm : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<LoginFormModel>
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
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
  
    bool incorrect = Model == null ? false : true;

#line default
#line hidden
            BeginContext(127, 25, true);
            WriteLiteral("<!doctype html>\r\n<html>\r\n");
            EndContext();
            BeginContext(152, 269, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("head", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "16508592b12844629d8078811a54d9dd", async() => {
                BeginContext(158, 13, true);
                WriteLiteral("\r\n    <title>");
                EndContext();
                BeginContext(172, 28, false);
#line 9 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
      Write(Html.Raw(localization.Title));

#line default
#line hidden
                EndContext();
                BeginContext(200, 214, true);
                WriteLiteral("</title>\r\n    <meta charset=\"utf-8\" />\r\n    <link rel=\"stylesheet\" type=\"text/css\" href=\"/styles/admin_panel/common.css\" />\r\n    <link rel=\"stylesheet\" type=\"text/css\" href=\"/styles/admin_panel/login_form.css\" />\r\n");
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
            BeginContext(421, 2, true);
            WriteLiteral("\r\n");
            EndContext();
            BeginContext(423, 618, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("body", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "91c9053bd1f3455e9056c0cc36e27d8d", async() => {
                BeginContext(429, 51, true);
                WriteLiteral("\r\n    <form method=\"post\" class=\"loginFormBlock\">\r\n");
                EndContext();
#line 16 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
         if (incorrect)
        {

#line default
#line hidden
                BeginContext(516, 35, true);
                WriteLiteral("            <span class=\"errorMsg\">");
                EndContext();
                BeginContext(552, 37, false);
#line 18 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
                              Write(Html.Raw(localization.IncorrectInput));

#line default
#line hidden
                EndContext();
                BeginContext(589, 9, true);
                WriteLiteral("</span>\r\n");
                EndContext();
#line 19 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
        }

#line default
#line hidden
                BeginContext(609, 27, true);
                WriteLiteral("        <label for=\"Login\">");
                EndContext();
                BeginContext(637, 31, false);
#line 20 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
                      Write(Html.Raw(localization.UserName));

#line default
#line hidden
                EndContext();
                BeginContext(668, 88, true);
                WriteLiteral("</label>\r\n        <input type=\"text\" id=\"Login\" name=\"Login\" autocomplete=\"off\" required");
                EndContext();
                BeginWriteAttribute("value", " value=\"", 756, "\"", 777, 1);
#line 21 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
WriteAttributeValue("", 764, Model?.Login, 764, 13, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(778, 33, true);
                WriteLiteral(">\r\n        <label for=\"Password\">");
                EndContext();
                BeginContext(812, 31, false);
#line 22 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
                         Write(Html.Raw(localization.Password));

#line default
#line hidden
                EndContext();
                BeginContext(843, 79, true);
                WriteLiteral("</label>\r\n        <input type=\"password\" id=\"Password\" name=\"Password\" required");
                EndContext();
                BeginWriteAttribute("value", " value=\"", 922, "\"", 946, 1);
#line 23 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
WriteAttributeValue("", 930, Model?.Password, 930, 16, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(947, 31, true);
                WriteLiteral(">\r\n        <input type=\"submit\"");
                EndContext();
                BeginWriteAttribute("value", " value=\"", 978, "\"", 1015, 1);
#line 24 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\LoginForm.cshtml"
WriteAttributeValue("", 986, Html.Raw(localization.Enter), 986, 29, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(1016, 18, true);
                WriteLiteral(" />\r\n    </form>\r\n");
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
            BeginContext(1041, 9, true);
            WriteLiteral("\r\n</html>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public ILoginFormLocalization localization { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<LoginFormModel> Html { get; private set; }
    }
}
#pragma warning restore 1591
