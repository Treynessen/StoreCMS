#pragma checksum "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\menu.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b1348b11a9943d5c3e83bb51443d682bec2378ac"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Chunks_menu), @"mvc.1.0.view", @"/Views/Chunks/menu.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Chunks/menu.cshtml", typeof(AspNetCore.Views_Chunks_menu))]
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
#line 1 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Functions;

#line default
#line hidden
#line 2 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Controllers;

#line default
#line hidden
#line 3 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.PagesManagement;

#line default
#line hidden
#line 4 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.ImagesManagement;

#line default
#line hidden
#line 5 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Database.Context;

#line default
#line hidden
#line 6 "D:\Users\trane\Desktop\github\StoreCMS\StoreCMS\Views\Chunks\_ViewImports.cshtml"
using Treynessen.Database.Entities;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"b1348b11a9943d5c3e83bb51443d682bec2378ac", @"/Views/Chunks/menu.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"4ea0ac9dd8e29f1d582e77aa90a16654937768a3", @"/Views/Chunks/_ViewImports.cshtml")]
    public class Views_Chunks_menu : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Page>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(12, 355, true);
            WriteLiteral(@"<div class=""menu-container"">
	<div id=""catalog-block"">
                <div class=""button"">Каталог товаров</div>
        </div>
	<div class=""main-menu"">
		<a href=""/"">Главная страница</a>
		<a href=""/contacts"">Контакты</a>
	</div>
	<div class=""search-block"">
		<input type=""text"" placeholder=""Поиск товара"">
		<input type=""submit"" value="""">
	</div>
</div>");
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
