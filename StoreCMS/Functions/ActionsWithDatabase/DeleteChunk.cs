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

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static void DeleteChunk(CMSDatabase db, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            Chunk deleteChunk = db.Chunks.FirstOrDefaultAsync(tc => tc.ID == itemID).Result;
            if (deleteChunk == null)
                return;
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string templateChunksPath = env.GetTemplateChunksPath();
            File.Delete($"{templateChunksPath}{deleteChunk.Name}.cshtml");
            db.Remove(deleteChunk);
            db.SaveChanges();

            var templatesTask = db.Templates.Where(t => t.TemplateSource.Contains($"[#{deleteChunk.Name}]")).AsNoTracking().ToListAsync();
            var chunksTask = db.Chunks.Where(tc => tc.TemplateSource.Contains($"[#{deleteChunk.Name}]")).AsNoTracking().ToListAsync();

            var task = Task.Run(() =>
            {
                foreach (var t in templatesTask.Result)
                    OtherFunctions.SourceToCSHTML(db, env.GetTemplatesPath(), t.Name, t.TemplateSource, env);
            });
            foreach (var c in chunksTask.Result)
                OtherFunctions.SourceToCSHTML(db, templateChunksPath, c.Name, c.TemplateSource, env);
            task.Wait();
        }
    }
}