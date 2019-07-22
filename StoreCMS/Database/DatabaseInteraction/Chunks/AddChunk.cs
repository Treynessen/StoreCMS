using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.TemplatesManagement;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void AddChunk(CMSDatabase db, TemplateModel model, HttpContext context, out bool successfullyCompleted)
        {
            Chunk chunk = TemplatesManagementFunctions.TemplateModelToITemplate<Chunk>(model, context);
            if (chunk == null)
            {
                successfullyCompleted = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            if (chunk.Name.Equals("_ViewImports", System.StringComparison.InvariantCultureIgnoreCase))
                chunk.Name = "view_imports";
            chunk.TemplatePath = env.GetChunksFolderShortPath();
            TemplatesManagementFunctions.SetUniqueITemplateName(db, chunk);
            chunk.TemplatePath += $"{chunk.Name}.cshtml";
            string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                db: db,
                source: chunk.TemplateSource,
                modelType: "Page",
                env: env,
                skipChunkName: chunk.Name
            );
            TemplatesManagementFunctions.WriteCshtmlContentToFile(
                pathToTemplatesFolder: env.GetChunksFolderFullPath(),
                templateName: chunk.Name,
                chstmlContent: cshtmlContent
            );
            chunk.ID = GetDatabaseRawID(db.Chunks);
            db.Chunks.Add(chunk);
            db.SaveChanges();
            model.ID = chunk.ID;
            successfullyCompleted = true;

            // Получаем список шаблонов и чанков, которые содержат данный чанк
            var templates = db.Templates.AsNoTracking().Where(t => t.TemplateSource.Contains($"[#{chunk.Name}]")).ToList();
            var chunks = db.Chunks.AsNoTracking().Where(tc => tc.ID != chunk.ID && (tc.TemplateSource.Contains($"[#{chunk.Name}]"))).ToList();
            // Делаем рендер, содержащих данный чанк, шаблонов и чанков 
            var renderTask = Task.Run(() =>
            {
                foreach (var t in templates)
                {
                    string _cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                        db: db,
                        source: t.TemplateSource,
                        modelType: "Page",
                        env: env,
                        skipChunkName: null
                    );
                    TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetTemplatesFolderFullPath(), t.Name, _cshtmlContent);
                }
            });
            foreach (var c in chunks)
            {
                string _cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                    db: db,
                    source: c.TemplateSource,
                    modelType: "Page",
                    env: env,
                    skipChunkName: c.Name
                );
                TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetChunksFolderFullPath(), c.Name, _cshtmlContent);
            }

            // Если шаблон блока товара содержит текущий чанк, то делаем его перерендер
            string productBlockFileContent = OtherFunctions.GetFileContent(env.GetProductBlockTemplateFullPath());
            if (productBlockFileContent.Contains($"[#{chunk.Name}]"))
            {
                string[] additions = {
                        "@using Treynessen.Functions;",
                        "@using Treynessen.Database.Entities;",
                        "@addTagHelper Treynessen.TagHelpers.ImageTagHelper, StoreCMS"
                };
                string productBlockCshtmlTemplate = TemplatesManagementFunctions.SourceToCSHTML(
                    db: db,
                    source: productBlockFileContent,
                    modelType: "ProductPage",
                    env: env,
                    skipChunkName: null,
                    additions: additions
                );
                using (StreamWriter writer = new StreamWriter(env.GetProductBlockCshtmlFullPath()))
                {
                    writer.Write(productBlockCshtmlTemplate);
                }
            }
            renderTask.Wait();
        }
    }
}