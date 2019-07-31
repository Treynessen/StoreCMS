using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Functions;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.TemplatesManagement;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void EditChunk(CMSDatabase db, int? itemID, TemplateModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (!itemID.HasValue || model == null)
            {
                successfullyCompleted = false;
                return;
            }
            Chunk editableChunk = db.Chunks.AsNoTracking().FirstOrDefault(t => t.ID == itemID);
            if (editableChunk == null)
            {
                successfullyCompleted = false;
                return;
            }
            Chunk editedChunk = TemplatesManagementFunctions.TemplateModelToITemplate<Chunk>(model, context);
            if (editedChunk == null)
            {
                successfullyCompleted = false;
                return;
            }
            if (editedChunk.Name.Equals("_ViewImports", StringComparison.OrdinalIgnoreCase))
                editedChunk.Name = "view_imports";
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            editedChunk.ID = itemID.Value;
            editedChunk.TemplatePath = env.GetChunksFolderShortPath();
            TemplatesManagementFunctions.SetUniqueITemplateName(db, editedChunk);
            editedChunk.TemplatePath += $"{editedChunk.Name}.cshtml";
            db.Chunks.Update(editedChunk);
            db.SaveChanges();
            successfullyCompleted = true;
            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{editableChunk.Name} (ID-{editableChunk.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.ChunkEdited}"
            );

            // Изменяем cshtml файл, если изменилось имя шаблона и/или код шаблона
            bool changedName = !editedChunk.Name.Equals(editableChunk.Name, StringComparison.Ordinal);
            bool changedTemplateSource = !editedChunk.TemplateSource.Equals(editableChunk.TemplateSource, StringComparison.Ordinal);
            if (changedName && changedTemplateSource)
            {
                string pathToOldFileName = $"{env.GetChunksFolderFullPath()}{editableChunk.Name}.cshtml";
                if (File.Exists(pathToOldFileName))
                    File.Delete(pathToOldFileName);
                string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                    db: db,
                    source: editedChunk.TemplateSource,
                    modelType: "Page",
                    env: env,
                    skipChunkName: editedChunk.Name
                );
                TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetChunksFolderFullPath(), editedChunk.Name, cshtmlContent);
            }
            else if (changedName)
            {
                string pathToOldFileName = $"{env.GetChunksFolderFullPath()}{editableChunk.Name}.cshtml";
                if (File.Exists(pathToOldFileName))
                    File.Move(pathToOldFileName, $"{env.GetChunksFolderFullPath()}{editedChunk.Name}.cshtml");
                else
                {
                    string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                        db: db,
                        source: editedChunk.TemplateSource,
                        modelType: "Page",
                        env: env,
                        skipChunkName: editedChunk.Name
                    );
                    TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetChunksFolderFullPath(), editedChunk.Name, cshtmlContent);
                }
            }
            else if (changedTemplateSource)
            {
                string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                    db: db,
                    source: editedChunk.TemplateSource,
                    modelType: "Page",
                    env: env,
                    skipChunkName: editedChunk.Name
                );
                TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetChunksFolderFullPath(), editedChunk.Name, cshtmlContent);
            }

            // Если изменилось название чанка, то получаем список шаблонов и чанков, которые использовали или теперь используют данный чанк
            // и делаем их перерендер
            if (changedName)
            {
                var templates = db.Templates.AsNoTracking()
                .Where(t => t.TemplateSource.Contains($"[#{editableChunk.Name}]") || t.TemplateSource.Contains($"[#{editedChunk.Name}]"))
                .ToList();
                var chunks = db.Chunks.AsNoTracking()
                .Where(tc => tc.ID != itemID.Value
                && (tc.TemplateSource.Contains($"[#{editableChunk.Name}]") || tc.TemplateSource.Contains($"[#{editedChunk.Name}]")))
                .ToList();
                var renderTask = Task.Run(() =>
                {
                    foreach (var t in templates)
                    {
                        string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                            db: db,
                            source: t.TemplateSource,
                            modelType: "Page",
                            env: env,
                            skipChunkName: null
                        );
                        TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetTemplatesFolderFullPath(), t.Name, cshtmlContent);
                    }
                });
                foreach (var c in chunks)
                {
                    string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                    db: db,
                    source: c.TemplateSource,
                    modelType: "Page",
                    env: env,
                    skipChunkName: c.Name
                );
                    TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetChunksFolderFullPath(), c.Name, cshtmlContent);
                }

                string productBlockFileContent = OtherFunctions.GetFileContent(env.GetProductBlockTemplateFullPath());
                if (changedName && productBlockFileContent.Contains($"[#{editableChunk.Name}]"))
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
}