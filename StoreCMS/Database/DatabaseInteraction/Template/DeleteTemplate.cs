using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
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
            Template template = db.Templates.FirstOrDefault(t => t.ID == itemID);
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
            var usualPages = db.UsualPages.Where(up => up.TemplateId == template.ID).ToList();
            var categoryPages = db.CategoryPages.Where(cp => cp.TemplateId == template.ID).ToList();
            var productPages = db.ProductPages.Where(pp => pp.TemplateId == template.ID).ToList();
            foreach (var up in usualPages)
                up.Template = null;
            foreach (var cp in categoryPages)
                cp.Template = null;
            foreach (var pp in productPages)
                pp.Template = null;

            db.Templates.Remove(template);
            db.SaveChanges();
            successfullyDeleted = true;
            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{template.Name} (ID-{template.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.TemplateDeleted}"
            );
        }
    }
}