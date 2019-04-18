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
            Template template = db.Templates.FirstOrDefault(t => t.ID == itemID);
            if (template == null)
                return;
            var usualPagesTask = db.UsualPages.Where(up => up.TemplateId == template.ID).ToListAsync();
            var categoryPagesTask = db.CategoryPages.Where(cp => cp.TemplateId == template.ID).ToListAsync();
            var productPagesTask = db.ProductPages.Where(pp => pp.TemplateId == template.ID).ToListAsync();
            foreach (var up in usualPagesTask.Result)
                up.Template = null;
            db.UsualPages.UpdateRange(usualPagesTask.Result);
            foreach (var cp in categoryPagesTask.Result)
                cp.Template = null;
            db.CategoryPages.UpdateRange(categoryPagesTask.Result);
            foreach (var pp in productPagesTask.Result)
                pp.Template = null;
            File.Delete($"{context.RequestServices.GetRequiredService<IHostingEnvironment>().ContentRootPath}/Views/Templates/{template.Name}.cshtml");
            db.ProductPages.UpdateRange(productPagesTask.Result);
            db.Templates.Remove(template);
            db.SaveChanges();
        }
    }
}