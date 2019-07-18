#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "94fa941dfd753362dfe3e71ead33c474959f7b8d"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_AdminPanel_Pages_EditPage), @"mvc.1.0.view", @"/Views/AdminPanel/Pages/EditPage.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/AdminPanel/Pages/EditPage.cshtml", typeof(AspNetCore.Views_AdminPanel_Pages_EditPage))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"94fa941dfd753362dfe3e71ead33c474959f7b8d", @"/Views/AdminPanel/Pages/EditPage.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"643dee66794a2698b58719ead6ad3b58418e7a0e", @"/Views/AdminPanel/_ViewImports.cshtml")]
    public class Views_AdminPanel_Pages_EditPage : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<PageModel>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
  
    Layout = "CommonPage";
    Context.Items["PageStyle"] = "/styles/admin_panel/pages/work_with_page.css";
    Context.Items["PageName"] = $"{localization.EditPage_PageName} {Model.PageName}";
    UsualPage[] pages = Context.Items["UsualPages"] as UsualPage[];
    Template[] templates = Context.Items["Templates"] as Template[];
    bool isMainPage = Context.Items["isMainPage"] == null ? false : (bool)Context.Items["isMainPage"];

#line default
#line hidden
            BeginContext(506, 336, true);
            WriteLiteral(@"<script src=""/scripts/admin_panel/send_data.js""></script>
            <script src=""/scripts/admin_panel/insert_tab.js""></script>
            <script src=""/scripts/admin_panel/checkbox_event_handler.js""></script>
            <form id=""edit-page-form"" class=""page-container"">
                <input id=""edit-page-button"" type=""submit""");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 842, "\"", 884, 1);
#line 15 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 850, Html.Raw(localization.SaveButton), 850, 34, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(885, 89, true);
            WriteLiteral(" />\r\n                <div class=\"blocks\">\r\n                    <div class=\"left-block\">\r\n");
            EndContext();
            BeginContext(1009, 43, true);
            WriteLiteral("                        <label for=\"Title\">");
            EndContext();
            BeginContext(1053, 28, false);
#line 19 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                      Write(Html.Raw(localization.Title));

#line default
#line hidden
            EndContext();
            BeginContext(1081, 96, true);
            WriteLiteral("*</label>\r\n                        <input type=\"text\" id=\"Title\" name=\"PageModel.Title\" required");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 1177, "\"", 1197, 1);
#line 20 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 1185, Model.Title, 1185, 12, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1198, 5, true);
            WriteLiteral(" />\r\n");
            EndContext();
            BeginContext(1243, 46, true);
            WriteLiteral("                        <label for=\"PageName\">");
            EndContext();
            BeginContext(1290, 33, false);
#line 22 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                         Write(Html.Raw(localization.Breadcrumb));

#line default
#line hidden
            EndContext();
            BeginContext(1323, 102, true);
            WriteLiteral("*</label>\r\n                        <input type=\"text\" id=\"PageName\" name=\"PageModel.PageName\" required");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 1425, "\"", 1448, 1);
#line 23 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 1433, Model.PageName, 1433, 15, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1449, 5, true);
            WriteLiteral(" />\r\n");
            EndContext();
            BeginContext(1500, 53, true);
            WriteLiteral("                        <label for=\"PageDescription\">");
            EndContext();
            BeginContext(1554, 38, false);
#line 25 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                                Write(Html.Raw(localization.PageDescription));

#line default
#line hidden
            EndContext();
            BeginContext(1592, 122, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"PageDescription\" name=\"PageModel.PageDescription\" maxlength=\"160\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 1714, "\"", 1744, 1);
#line 26 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 1722, Model.PageDescription, 1722, 22, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1745, 3, true);
            WriteLiteral(">\r\n");
            EndContext();
            BeginContext(1791, 50, true);
            WriteLiteral("                        <label for=\"PageKeywords\">");
            EndContext();
            BeginContext(1842, 35, false);
#line 28 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                             Write(Html.Raw(localization.PageKeywords));

#line default
#line hidden
            EndContext();
            BeginContext(1877, 100, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"PageKeywords\" name=\"PageModel.PageKeywords\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 1977, "\"", 2004, 1);
#line 29 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 1985, Model.PageKeywords, 1985, 19, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2005, 80, true);
            WriteLiteral(" />\r\n                    </div>\r\n                    <div class=\"right-block\">\r\n");
            EndContext();
            BeginContext(2123, 48, true);
            WriteLiteral("                        <label for=\"TemplateId\">");
            EndContext();
            BeginContext(2172, 31, false);
#line 33 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                           Write(Html.Raw(localization.Template));

#line default
#line hidden
            EndContext();
            BeginContext(2203, 97, true);
            WriteLiteral("</label>\r\n                        <select id=\"TemplateId\" name=\"PageModel.TemplateId\" required>\r\n");
            EndContext();
#line 35 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         if (!Model.TemplateId.HasValue)
                        {

#line default
#line hidden
            BeginContext(2385, 45, true);
            WriteLiteral("                            <option selected>");
            EndContext();
            BeginContext(2431, 38, false);
#line 37 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                        Write(Html.Raw(localization.WithoutTemplate));

#line default
#line hidden
            EndContext();
            BeginContext(2469, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 38 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }
                        else
                        {

#line default
#line hidden
            BeginContext(2564, 36, true);
            WriteLiteral("                            <option>");
            EndContext();
            BeginContext(2601, 38, false);
#line 41 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                               Write(Html.Raw(localization.WithoutTemplate));

#line default
#line hidden
            EndContext();
            BeginContext(2639, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 42 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }

#line default
#line hidden
            BeginContext(2677, 24, true);
            WriteLiteral("                        ");
            EndContext();
#line 43 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         foreach (var t in templates)
                        {
                            if (Model.TemplateId.HasValue && t.ID == Model.TemplateId.Value)
                            {

#line default
#line hidden
            BeginContext(2884, 35, true);
            WriteLiteral("                            <option");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 2919, "\"", 2932, 1);
#line 47 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 2927, t.ID, 2927, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(2933, 10, true);
            WriteLiteral(" selected>");
            EndContext();
            BeginContext(2944, 16, false);
#line 47 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                                      Write(Html.Raw(t.Name));

#line default
#line hidden
            EndContext();
            BeginContext(2960, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 48 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                            }
                            else
                            {

#line default
#line hidden
            BeginContext(3067, 35, true);
            WriteLiteral("                            <option");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 3102, "\"", 3115, 1);
#line 51 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 3110, t.ID, 3110, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(3116, 1, true);
            WriteLiteral(">");
            EndContext();
            BeginContext(3118, 16, false);
#line 51 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                             Write(Html.Raw(t.Name));

#line default
#line hidden
            EndContext();
            BeginContext(3134, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 52 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                            }
                        }

#line default
#line hidden
            BeginContext(3203, 35, true);
            WriteLiteral("                        </select>\r\n");
            EndContext();
#line 55 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         if (!isMainPage)
                        {
                        

#line default
#line hidden
            BeginContext(3351, 50, true);
            WriteLiteral("                        <label for=\"PreviousPage\">");
            EndContext();
            BeginContext(3402, 35, false);
#line 58 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                             Write(Html.Raw(localization.PreviousPage));

#line default
#line hidden
            EndContext();
            BeginContext(3437, 103, true);
            WriteLiteral("</label>\r\n                        <select id=\"PreviousPage\" name=\"PageModel.PreviousPageID\" required>\r\n");
            EndContext();
#line 60 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         if (!Model.PreviousPageID.HasValue)
                        {

#line default
#line hidden
            BeginContext(3629, 45, true);
            WriteLiteral("                            <option selected>");
            EndContext();
            BeginContext(3675, 42, false);
#line 62 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                        Write(Html.Raw(localization.WithoutPreviousPage));

#line default
#line hidden
            EndContext();
            BeginContext(3717, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 63 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }
                        else
                        {

#line default
#line hidden
            BeginContext(3812, 36, true);
            WriteLiteral("                            <option>");
            EndContext();
            BeginContext(3849, 42, false);
#line 66 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                               Write(Html.Raw(localization.WithoutPreviousPage));

#line default
#line hidden
            EndContext();
            BeginContext(3891, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 67 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }

#line default
#line hidden
            BeginContext(3929, 24, true);
            WriteLiteral("                        ");
            EndContext();
#line 68 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         foreach (var p in pages)
                        {
                            if (Model.PreviousPageID == p.PreviousPageID && Model.Alias.Equals(p.Alias, StringComparison.InvariantCulture))
                            {
                                continue;
                            }
                            if (Model.PreviousPageID.HasValue && p.ID == Model.PreviousPageID.Value)
                            {

#line default
#line hidden
            BeginContext(4386, 35, true);
            WriteLiteral("                            <option");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 4421, "\"", 4434, 1);
#line 76 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 4429, p.ID, 4429, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(4435, 10, true);
            WriteLiteral(" selected>");
            EndContext();
            BeginContext(4446, 20, false);
#line 76 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                                      Write(Html.Raw(p.PageName));

#line default
#line hidden
            EndContext();
            BeginContext(4466, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 77 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                            }
                            else
                            {

#line default
#line hidden
            BeginContext(4573, 35, true);
            WriteLiteral("                            <option");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 4608, "\"", 4621, 1);
#line 80 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 4616, p.ID, 4616, 5, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(4622, 1, true);
            WriteLiteral(">");
            EndContext();
            BeginContext(4624, 20, false);
#line 80 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                             Write(Html.Raw(p.PageName));

#line default
#line hidden
            EndContext();
            BeginContext(4644, 11, true);
            WriteLiteral("</option>\r\n");
            EndContext();
#line 81 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                            }
                        }

#line default
#line hidden
            BeginContext(4713, 35, true);
            WriteLiteral("                        </select>\r\n");
            EndContext();
            BeginContext(4783, 43, true);
            WriteLiteral("                        <label for=\"Alias\">");
            EndContext();
            BeginContext(4827, 28, false);
#line 85 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                      Write(Html.Raw(localization.Alias));

#line default
#line hidden
            EndContext();
            BeginContext(4855, 86, true);
            WriteLiteral("</label>\r\n                        <input type=\"text\" id=\"Alias\" name=\"PageModel.Alias\"");
            EndContext();
            BeginWriteAttribute("value", " value=\"", 4941, "\"", 4962, 1);
#line 86 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
WriteAttributeValue("", 4949, Model?.Alias, 4949, 13, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(4963, 5, true);
            WriteLiteral(" />\r\n");
            EndContext();
#line 87 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }
                        

#line default
#line hidden
            BeginContext(5038, 54, true);
            WriteLiteral("                        <div class=\"checkbox-block\">\r\n");
            EndContext();
#line 90 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         if (Model.Published)
                        {

#line default
#line hidden
            BeginContext(5166, 128, true);
            WriteLiteral("                            <input type=\"checkbox\" id=\"Published\" name=\"PageModel.Published\" value=\"true\" checked=\"checked\" />\r\n");
            EndContext();
#line 93 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }
                        else
                        {

#line default
#line hidden
            BeginContext(5378, 111, true);
            WriteLiteral("                            <input type=\"checkbox\" id=\"Published\" name=\"PageModel.Published\" value=\"false\" />\r\n");
            EndContext();
#line 97 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }

#line default
#line hidden
            BeginContext(5516, 323, true);
            WriteLiteral(@"                            <script>
                                let publishedCheckbox = document.getElementById('Published');
                                publishedCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""Published"">");
            EndContext();
            BeginContext(5840, 32, false);
#line 102 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                              Write(Html.Raw(localization.Published));

#line default
#line hidden
            EndContext();
            BeginContext(5872, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(5959, 54, true);
            WriteLiteral("                        <div class=\"checkbox-block\">\r\n");
            EndContext();
#line 106 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         if (Model.IsIndex)
                        {

#line default
#line hidden
            BeginContext(6085, 124, true);
            WriteLiteral("                            <input type=\"checkbox\" id=\"IsIndex\" name=\"PageModel.IsIndex\" value=\"true\" checked=\"checked\" />\r\n");
            EndContext();
#line 109 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }
                        else
                        {

#line default
#line hidden
            BeginContext(6293, 107, true);
            WriteLiteral("                            <input type=\"checkbox\" id=\"IsIndex\" name=\"PageModel.IsIndex\" value=\"false\" />\r\n");
            EndContext();
#line 113 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }

#line default
#line hidden
            BeginContext(6427, 315, true);
            WriteLiteral(@"                            <script>
                                let isIndexCheckbox = document.getElementById('IsIndex');
                                isIndexCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""IsIndex"">");
            EndContext();
            BeginContext(6743, 30, false);
#line 118 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                            Write(Html.Raw(localization.IsIndex));

#line default
#line hidden
            EndContext();
            BeginContext(6773, 42, true);
            WriteLiteral("</label>\r\n                        </div>\r\n");
            EndContext();
            BeginContext(6855, 54, true);
            WriteLiteral("                        <div class=\"checkbox-block\">\r\n");
            EndContext();
#line 122 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                         if (Model.IsFollow)
                        {

#line default
#line hidden
            BeginContext(6982, 126, true);
            WriteLiteral("                            <input type=\"checkbox\" id=\"IsFollow\" name=\"PageModel.IsFollow\" value=\"true\" checked=\"checked\" />\r\n");
            EndContext();
#line 125 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }
                        else
                        {

#line default
#line hidden
            BeginContext(7192, 109, true);
            WriteLiteral("                            <input type=\"checkbox\" id=\"IsFollow\" name=\"PageModel.IsFollow\" value=\"false\" />\r\n");
            EndContext();
#line 129 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                        }

#line default
#line hidden
            BeginContext(7328, 319, true);
            WriteLiteral(@"                            <script>
                                let isFollowCheckbox = document.getElementById('IsFollow');
                                isFollowCheckbox.addEventListener('click', checkboxEventHandler);
                            </script>
                            <label for=""IsFollow"">");
            EndContext();
            BeginContext(7648, 31, false);
#line 134 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                             Write(Html.Raw(localization.IsFollow));

#line default
#line hidden
            EndContext();
            BeginContext(7679, 94, true);
            WriteLiteral("</label>\r\n                        </div>\r\n                    </div>\r\n                </div>\r\n");
            EndContext();
            BeginContext(7802, 37, true);
            WriteLiteral("                <label for=\"Content\">");
            EndContext();
            BeginContext(7840, 30, false);
#line 139 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                Write(Html.Raw(localization.Content));

#line default
#line hidden
            EndContext();
            BeginContext(7870, 74, true);
            WriteLiteral("</label>\r\n                <textarea id=\"Content\" name=\"PageModel.Content\">");
            EndContext();
            BeginContext(7945, 14, false);
#line 140 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                                           Write(Model?.Content);

#line default
#line hidden
            EndContext();
            BeginContext(7959, 222, true);
            WriteLiteral("</textarea>\r\n            </form>\r\n            <script>\r\n                function errorHandler(formElement) {\r\n                    let errorMsg = document.createElement(\'span\');\r\n                    errorMsg.textContent = \'");
            EndContext();
            BeginContext(8182, 37, false);
#line 145 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                       Write(Html.Raw(localization.IncorrectInput));

#line default
#line hidden
            EndContext();
            BeginContext(8219, 246, true);
            WriteLiteral("\';\r\n                    errorMsg.setAttribute(\'id\', \'error-msg\');\r\n                    formElement.insertBefore(errorMsg, formElement.firstChild);\r\n                }\r\n                function successRequestHandler() {\r\n                    alert(\'");
            EndContext();
            BeginContext(8466, 33, false);
#line 150 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                      Write(Html.Raw(localization.PageEdited));

#line default
#line hidden
            EndContext();
            BeginContext(8499, 108, true);
            WriteLiteral("\');\r\n                    location.reload();\r\n                }\r\n                let searchString = \'?pageID=");
            EndContext();
            BeginContext(8609, 29, false);
#line 153 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                        Write((int)AdminPanelPages.EditPage);

#line default
#line hidden
            EndContext();
            BeginContext(8639, 8, true);
            WriteLiteral("&itemID=");
            EndContext();
            BeginContext(8648, 8, false);
#line 153 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\AdminPanel\Pages\EditPage.cshtml"
                                                                               Write(Model.ID);

#line default
#line hidden
            EndContext();
            BeginContext(8656, 418, true);
            WriteLiteral(@"';
                let editPageButton = document.getElementById('edit-page-button');
                editPageButton.addEventListener('click', createSendDataEventHandler('POST', searchString, 'edit-page-form', successRequestHandler, errorHandler));
                let textarea = document.getElementById('Content');
                textarea.addEventListener('keydown', insertTabEventHandler);
            </script>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IPagesLocalization localization { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<PageModel> Html { get; private set; }
    }
}
#pragma warning restore 1591
