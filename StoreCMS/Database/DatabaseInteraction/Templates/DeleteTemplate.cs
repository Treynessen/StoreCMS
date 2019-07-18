using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteTemplate(CMSDatabase db, int? itemID, HttpContext context, out bool successfullyDeleted)
        {
            if (!itemID.HasValue)
            {
                successfullyDeleted = false;
                return;
            }
            Template template = db.Templates.FirstOrDefaultAsync(t => t.ID == itemID).Result;
            if (template == null)
            {
                successfullyDeleted = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string pathToTemplateFile = $"{env.GetTemplatesFolderFullPath()}{template.Name}.cshtml";
            if (File.Exists(pathToTemplateFile))
                File.Delete(pathToTemplateFile);

            // Удаляем ссылку на шаблон у страниц, которые использовали его
            var usualPagesTask = db.UsualPages.Where(up => up.TemplateId == template.ID).ToListAsync();
            var categoryPagesTask = db.CategoryPages.Where(cp => cp.TemplateId == template.ID).ToListAsync();
            var productPagesTask = db.ProductPages.Where(pp => pp.TemplateId == template.ID).ToListAsync();
            var usualPagesChangeTask = Task.Run(() =>
            {
                foreach (var up in usualPagesTask.Result)
                    up.Template = null;
            });
            var categoryPagesChangeTask = Task.Run(() =>
            {
                foreach (var cp in categoryPagesTask.Result)
                    cp.Template = null;
            });
            foreach (var pp in productPagesTask.Result)
                pp.Template = null;
            usualPagesChangeTask.Wait();
            categoryPagesChangeTask.Wait();

            db.Templates.Remove(template);
            db.SaveChanges();
            successfullyDeleted = true;
        }
    }
}