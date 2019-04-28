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
        public static void DeleteTemplateChunk(CMSDatabase db, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            TemplateChunk deleteChunk = db.TemplateChunks.FirstOrDefaultAsync(tc => tc.ID == itemID).Result;
            if (deleteChunk == null)
                return;
            IHostingEnvironment env = context.RequestServices.GetRequiredService<IHostingEnvironment>();
            string templateChunksPath = env.GetTemplateChunksPath();
            File.Delete($"{templateChunksPath}{deleteChunk.Name}.cshtml");
            db.Remove(deleteChunk);
            db.SaveChanges();

            var templatesTask = db.Templates.Where(t => t.TemplateSource.Contains($"[#{deleteChunk.Name}]")).ToListAsync();
            var chunksTask = db.TemplateChunks.Where(tc => tc.TemplateSource.Contains($"[#{deleteChunk.Name}]")).ToListAsync();

            var task = Task.Run(() =>
            {
                foreach (var t in templatesTask.Result)
                    OtherFunctions.SourceToCSHTML(db, env.GetTemplatesPath(), t.Name, t.TemplateSource);
                db.Templates.UpdateRange(templatesTask.Result);
            });
            foreach (var c in chunksTask.Result)
                OtherFunctions.SourceToCSHTML(db, templateChunksPath, c.Name, c.TemplateSource);
            task.Wait();
            db.TemplateChunks.UpdateRange(chunksTask.Result);
            db.SaveChanges();
        }
    }
}