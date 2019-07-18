#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2cf6beb874d5d1ec50e6b13371ebf16398d71d15"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_AdminPanel_Templates_AddTemplate), @"mvc.1.0.view", @"/Views/AdminPanel/Templates/AddTemplate.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/AdminPanel/Templates/AddTemplate.cshtml", typeof(AspNetCore.Views_AdminPanel_Templates_AddTemplate))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"2cf6beb874d5d1ec50e6b13371ebf16398d71d15", @"/Views/AdminPanel/Templates/AddTemplate.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"643dee66794a2698b58719ead6ad3b58418e7a0e", @"/Views/AdminPanel/_ViewImports.cshtml")]
    public class Views_AdminPanel_Templates_AddTemplate : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml"
  
    Layout = "CommonPage";
    Context.Items["PageStyle"] = "/styles/admin_panel/templates/work_with_template.css";
    Context.Items["PageName"] = localization.AddTemplatePageName;

#line default
#line hidden
            BeginContext(237, 277, true);
            WriteLiteral(@"<script src=""/scripts/admin_panel/send_data.js""></script>
            <script src=""/scripts/admin_panel/insert_tab.js""></script>
            <form id=""add-template-form"" class=""page-container"">
                <div class=""name-block"">
                    <label for=""Name"">");
            EndContext();
            BeginContext(515, 27, false);
#line 11 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml"
                                 Write(Html.Raw(localization.Name));

#line default
#line hidden
            EndContext();
            BeginContext(542, 166, true);
            WriteLiteral("</label>\r\n                    <input type=\"text\" id=\"Name\" name=\"TemplateModel.Name\" required />\r\n                </div>\r\n                <label for=\"TemplateSource\">");
            EndContext();
            BeginContext(709, 35, false);
#line 14 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml"
                                       Write(Html.Raw(localization.TemplateCode));

#line default
#line hidden
            EndContext();
            BeginContext(744, 166, true);
            WriteLiteral("</label>\r\n                <textarea id=\"TemplateSource\" name=\"TemplateModel.TemplateSource\"></textarea>\r\n                <input id=\"add-template-button\" type=\"submit\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 910, "\"", 952, 1);
#line 16 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml"
WriteAttributeValue("", 918, Html.Raw(localization.SaveButton), 918, 34, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(953, 214, true);
            WriteLiteral(" />\r\n            </form>\r\n            <script>\r\n                function errorHandler(formElement) {\r\n                    let errorMsg = document.createElement(\'span\');\r\n                    errorMsg.textContent = \'");
            EndContext();
            BeginContext(1168, 37, false);
#line 21 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml"
                                       Write(Html.Raw(localization.IncorrectInput));

#line default
#line hidden
            EndContext();
            BeginContext(1205, 256, true);
            WriteLiteral(@"';
                    errorMsg.setAttribute('id', 'error-msg');
                    formElement.insertBefore(errorMsg, formElement.firstChild);
                }
                function successfulRequestHandler(request) {
                    alert('");
            EndContext();
            BeginContext(1462, 38, false);
#line 26 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml"
                      Write(Html.Raw(localization.TemplateCreated));

#line default
#line hidden
            EndContext();
            BeginContext(1500, 295, true);
            WriteLiteral(@"');
                    location.replace(request.getResponseHeader('location'));
                }
                let addTemplateButton = document.getElementById('add-template-button');
                addTemplateButton.addEventListener('click', createSendDataEventHandler('POST', '?pageID=");
            EndContext();
            BeginContext(1797, 32, false);
#line 30 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\AddTemplate.cshtml"
                                                                                                    Write((int)AdminPanelPages.AddTemplate);

#line default
#line hidden
            EndContext();
            BeginContext(1830, 241, true);
            WriteLiteral("\', \'add-template-form\', successfulRequestHandler, errorHandler));\r\n                let textarea = document.getElementById(\'TemplateSource\');\r\n                textarea.addEventListener(\'keydown\', insertTabEventHandler);\r\n            </script>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public ITemplatesLocalization localization { get; private set; }
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
