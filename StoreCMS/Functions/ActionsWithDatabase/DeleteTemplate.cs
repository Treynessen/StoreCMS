using System.IO;
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
            Template template = db.Templates.FirstOrDefaultAsync(t => t.ID == itemID).Result;
            File.Delete($"{context.RequestServices.GetRequiredService<IHostingEnvironment>().ContentRootPath}/Views/Templates/{template.Name}.cshtml");
            db.Templates.Remove(template);
            db.SaveChanges();
        }
    }
}