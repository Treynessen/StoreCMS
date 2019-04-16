using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static void DeleteTemplate(CMSDatabase db, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            List<UsualPage> usualPages = db.UsualPages.Where(up => up.TemplateId == itemID).ToList();
            List<CategoryPage> categoryPages = db.CategoryPages.Where(cp => cp.TemplateId == itemID).ToList();
            List<ProductPage> productPages = db.ProductPages.Where(pp => pp.TemplateId == itemID).ToList();
            foreach (var up in usualPages)
                up.Template = null;
            db.UsualPages.UpdateRange(usualPages);
            foreach (var cp in categoryPages)
                cp.Template = null;
            db.CategoryPages.UpdateRange(categoryPages);
            foreach (var pp in productPages)
                pp.Template = null;
            db.ProductPages.UpdateRange(productPages);
            Template template = db.Templates.FirstOrDefaultAsync(t => t.ID == itemID).Result;
            File.Delete($"{context.RequestServices.GetRequiredService<IHostingEnvironment>().ContentRootPath}/Views/Templates/{template.Name}.cshtml");
            db.Templates.Remove(template);
            db.SaveChanges();
        }
    }
}