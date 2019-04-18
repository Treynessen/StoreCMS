using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool AddTemplateChunk(CMSDatabase db, TemplateModel model, HttpContext context)
        {
            TemplateChunk chunk = OtherFunctions.TemplateModelToITemplate<TemplateChunk>(model, context);
            if (chunk == null)
                return false;
            if (string.IsNullOrEmpty(chunk.TemplatePath))
                chunk.TemplatePath = $"~/Views/TemplateChunks/";
            if (!Validator.TryValidateObject(chunk, new ValidationContext(chunk), null))
                return false;

            string pathToTemplateChunks = $"{context.RequestServices.GetService<IHostingEnvironment>().ContentRootPath}/Views/TemplateChunks";
            if (chunk.Name.Equals("_ViewImports", System.StringComparison.CurrentCultureIgnoreCase))
                chunk.Name = "view_imports";
            OtherFunctions.SetUniqueITemplateName(db, chunk);
            chunk.TemplatePath += $"{chunk.Name}.cshtml";
            OtherFunctions.SourceToCSHTML(db, pathToTemplateChunks, chunk.Name, chunk.TemplateSource);
            chunk.ID = OtherFunctions.GetDatabaseRawID(db.TemplateChunks);
            db.TemplateChunks.Add(chunk);
            db.SaveChanges();

            var templates = db.Templates.Where(t => t.TemplateSource.Contains($"[#{chunk.Name}]")).ToListAsync();
            var chunks = db.TemplateChunks.Where(tc => tc.TemplateSource.Contains($"[#{chunk.Name}]")).ToListAsync();
            var task = Task.Run(() =>
            {
                string pathToTemplates = $"{pathToTemplateChunks.Substring(0, pathToTemplateChunks.Length - "/TemplateChunks".Length)}/Templates";
                foreach (var t in templates.Result)
                    OtherFunctions.SourceToCSHTML(db, pathToTemplates, t.Name, t.TemplateSource);
            });
            foreach (var tc in chunks.Result)
                OtherFunctions.SourceToCSHTML(db, pathToTemplateChunks, tc.Name, tc.TemplateSource);
            task.Wait();
            return true;
        }
    }
}