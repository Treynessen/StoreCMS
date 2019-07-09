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
using Treynessen.TemplatesManagement;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void DeleteChunk(CMSDatabase db, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            Chunk chunk = db.Chunks.FirstOrDefaultAsync(t => t.ID == itemID).Result;
            if (chunk == null)
                return;
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string pathToChunkFile = $"{env.GetChunksFolderFullPath()}{chunk.Name}.cshtml";
            if (File.Exists(pathToChunkFile))
                File.Delete(pathToChunkFile);
            db.Chunks.Remove(chunk);
            db.SaveChanges();

            // Получаем список чанков и шаблонов, использующих данный чанк и делаем перерендер
            var templates = db.Templates
            .Where(t => t.TemplateSource.Contains($"[#{chunk.Name}]"))
            .AsNoTracking()
            .ToList();
            var chunks = db.Chunks
            .Where(tc => tc.TemplateSource.Contains($"[#{chunk.Name}]"))
            .AsNoTracking()
            .ToList();
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
            renderTask.Wait();
        }
    }
}