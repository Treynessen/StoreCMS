#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "209e794909d940e8a2d831604a4646ef37a9fd94"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_TemplateChunks_products_page_content), @"mvc.1.0.view", @"/Views/TemplateChunks/products_page_content.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/TemplateChunks/products_page_content.cshtml", typeof(AspNetCore.Views_TemplateChunks_products_page_content))]
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
#line 1 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\_ViewImports.cshtml"
using Functions = Treynessen.Functions.ViewFunctions;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"209e794909d940e8a2d831604a4646ef37a9fd94", @"/Views/TemplateChunks/products_page_content.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e41fce856d5d6b47c759492598140418e08f23b3", @"/Views/TemplateChunks/_ViewImports.cshtml")]
    public class Views_TemplateChunks_products_page_content : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Page>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml"
  
	List<ProductPage> products = Context.Items["products"] as List<ProductPage>;

#line default
#line hidden
            BeginContext(95, 28, true);
            WriteLiteral("<div class=\"contentBlock\">\r\n");
            EndContext();
            BeginContext(125, 32, false);
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml"
Write(Html.Raw(Model?.BreadcrumbsHtml));

#line default
#line hidden
            EndContext();
            BeginContext(158, 3, true);
            WriteLiteral(" → ");
            EndContext();
            BeginContext(163, 25, false);
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml"
                                  Write(Html.Raw(Model?.PageName));

#line default
#line hidden
            EndContext();
            BeginContext(189, 2, true);
            WriteLiteral("\r\n");
            EndContext();
            BeginContext(193, 24, false);
#line 7 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml"
Write(Html.Raw(Model?.Content));

#line default
#line hidden
            EndContext();
            BeginContext(218, 2, true);
            WriteLiteral("\r\n");
            EndContext();
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml"
  if(products != null) { foreach(var p in products) {

#line default
#line hidden
            BeginContext(274, 21, false);
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml"
                                                Write(Html.Raw(p?.PageName));

#line default
#line hidden
            EndContext();
#line 8 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\TemplateChunks\products_page_content.cshtml"
                                                                           } }

#line default
#line hidden
            BeginContext(299, 8, true);
            WriteLiteral("\r\n</div>");
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
