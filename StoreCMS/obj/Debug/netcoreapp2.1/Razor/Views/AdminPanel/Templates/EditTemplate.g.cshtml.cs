#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "6b203b0c296b10ccb5c41086e91f577b568252d4"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_AdminPanel_Templates_EditTemplate), @"mvc.1.0.view", @"/Views/AdminPanel/Templates/EditTemplate.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/AdminPanel/Templates/EditTemplate.cshtml", typeof(AspNetCore.Views_AdminPanel_Templates_EditTemplate))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"6b203b0c296b10ccb5c41086e91f577b568252d4", @"/Views/AdminPanel/Templates/EditTemplate.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"643dee66794a2698b58719ead6ad3b58418e7a0e", @"/Views/AdminPanel/_ViewImports.cshtml")]
    public class Views_AdminPanel_Templates_EditTemplate : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<TemplateModel>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
  
    Layout = "CommonPage";
    Context.Items["PageStyle"] = "/styles/admin_panel/templates/work_with_template.css";
    Context.Items["PageName"] = $"{localization.EditTemplatePageName} {Model?.Name}";

#line default
#line hidden
            BeginContext(279, 278, true);
            WriteLiteral(@"<script src=""/scripts/admin_panel/send_data.js""></script>
            <script src=""/scripts/admin_panel/insert_tab.js""></script>
            <form id=""edit-template-form"" class=""page-container"">
                <div class=""name-block"">
                    <label for=""Name"">");
            EndContext();
            BeginContext(558, 27, false);
#line 12 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
                                 Write(Html.Raw(localization.Name));

#line default
#line hidden
            EndContext();
            BeginContext(585, 84, true);
            WriteLiteral("</label>\r\n                    <input type=\"text\" id=\"Name\" name=\"TemplateModel.Name\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 669, "\"", 699, 1);
#line 13 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
WriteAttributeValue("", 677, Html.Raw(Model?.Name), 677, 22, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(700, 82, true);
            WriteLiteral(" required />\r\n                </div>\r\n                <label for=\"TemplateSource\">");
            EndContext();
            BeginContext(783, 35, false);
#line 15 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
                                       Write(Html.Raw(localization.TemplateCode));

#line default
#line hidden
            EndContext();
            BeginContext(818, 92, true);
            WriteLiteral("</label>\r\n                <textarea id=\"TemplateSource\" name=\"TemplateModel.TemplateSource\">");
            EndContext();
            BeginContext(911, 31, false);
#line 16 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
                                                                             Write(Html.Raw(Model?.TemplateSource));

#line default
#line hidden
            EndContext();
            BeginContext(942, 75, true);
            WriteLiteral("</textarea>\r\n                <input id=\"edit-template-button\" type=\"submit\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 1017, "\"", 1059, 1);
#line 17 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
WriteAttributeValue("", 1025, Html.Raw(localization.SaveButton), 1025, 34, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1060, 214, true);
            WriteLiteral(" />\r\n            </form>\r\n            <script>\r\n                function errorHandler(formElement) {\r\n                    let errorMsg = document.createElement(\'span\');\r\n                    errorMsg.textContent = \'");
            EndContext();
            BeginContext(1275, 37, false);
#line 22 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
                                       Write(Html.Raw(localization.IncorrectInput));

#line default
#line hidden
            EndContext();
            BeginContext(1312, 246, true);
            WriteLiteral("\';\r\n                    errorMsg.setAttribute(\'id\', \'error-msg\');\r\n                    formElement.insertBefore(errorMsg, formElement.firstChild);\r\n                }\r\n                function successRequestHandler() {\r\n                    alert(\'");
            EndContext();
            BeginContext(1559, 37, false);
#line 27 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
                      Write(Html.Raw(localization.TemplateEdited));

#line default
#line hidden
            EndContext();
            BeginContext(1596, 260, true);
            WriteLiteral(@"');
                    location.reload();
                }
                let editTemplateButton = document.getElementById('edit-template-button');
                editTemplateButton.addEventListener('click', createSendDataEventHandler('POST', '?pageID=");
            EndContext();
            BeginContext(1858, 33, false);
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
                                                                                                     Write((int)AdminPanelPages.EditTemplate);

#line default
#line hidden
            EndContext();
            BeginContext(1892, 8, true);
            WriteLiteral("&itemID=");
            EndContext();
            BeginContext(1901, 8, false);
#line 31 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Templates\EditTemplate.cshtml"
                                                                                                                                                Write(Model.ID);

#line default
#line hidden
            EndContext();
            BeginContext(1909, 239, true);
            WriteLiteral("\', \'edit-template-form\', successRequestHandler, errorHandler));\r\n                let textarea = document.getElementById(\'TemplateSource\');\r\n                textarea.addEventListener(\'keydown\', insertTabEventHandler);\r\n            </script>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TemplateModel> Html { get; private set; }
    }
}
#pragma warning restore 1591
