#pragma checksum "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\content.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5be8ed1d73254ad71c06c9c1a28d743d107cf4ea"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Chunks_content), @"mvc.1.0.view", @"/Views/Chunks/content.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Chunks/content.cshtml", typeof(AspNetCore.Views_Chunks_content))]
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
#line 1 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Functions;

#line default
#line hidden
#line 2 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Controllers;

#line default
#line hidden
#line 3 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.PagesManagement;

#line default
#line hidden
#line 4 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.ImagesManagement;

#line default
#line hidden
#line 5 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 6 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"5be8ed1d73254ad71c06c9c1a28d743d107cf4ea", @"/Views/Chunks/content.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a171fe621900d30ce349d435054eba87062936fb", @"/Views/Chunks/_ViewImports.cshtml")]
    public class Views_Chunks_content : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Page>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(12, 28, true);
            WriteLiteral("<div class=\"content-block\">\n");
            EndContext();
            BeginContext(42, 64, false);
#line 3 "C:\Users\79622\Desktop\Github\StoreCMS\StoreCMS\Views\Chunks\content.cshtml"
Write(Model != null ? Html.Raw(Model.Content) : Html.Raw(string.Empty));

#line default
#line hidden
            EndContext();
            BeginContext(107, 8, true);
            WriteLiteral("\n</div>\n");
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
