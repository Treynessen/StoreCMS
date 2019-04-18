using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public static void DeleteTemplateChunk(CMSDatabase db, int? itemID, HttpContext context)
        {
            if (!itemID.HasValue)
                return;
            TemplateChunk deleteChunk = db.TemplateChunks.FirstOrDefault(tc => tc.ID == itemID);
            if (deleteChunk == null)
                return;
            string pathToViews = $"{context.RequestServices.GetRequiredService<IHostingEnvironment>().ContentRootPath}/Views";
            File.Delete($"{pathToViews}/TemplateChunks/{deleteChunk.Name}.cshtml");
            db.Remove(deleteChunk);
            db.SaveChanges();

            var templatesTask = db.Templates.Where(t => t.TemplateSource.Contains($"[#{deleteChunk.Name}]")).ToListAsync();
            var chunksTask = db.TemplateChunks.Where(tc => tc.TemplateSource.Contains($"[#{deleteChunk.Name}]")).ToListAsync();

            var task = Task.Run(() =>
            {
                foreach (var t in templatesTask.Result)
                    OtherFunctions.SourceToCSHTML(db, $"{pathToViews}/Templates", t.Name, t.TemplateSource);
                db.Templates.UpdateRange(templatesTask.Result);
            });
            foreach (var c in chunksTask.Result)
                OtherFunctions.SourceToCSHTML(db, $"{pathToViews}/TemplateChunks", c.Name, c.TemplateSource);
            task.Wait();
            db.TemplateChunks.UpdateRange(chunksTask.Result);
            db.SaveChanges();
        }
    }
}