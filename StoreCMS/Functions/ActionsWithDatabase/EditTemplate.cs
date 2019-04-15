using System.IO;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool EditTemplate(CMSDatabase db, AdminPanelModel model, HttpContext context)
        {
            if (!model.itemID.HasValue || model.TemplateModel == null)
                return false;
            Template changeTemplate = db.Templates.FirstOrDefaultAsync(t => t.ID == model.itemID).Result;
            if (changeTemplate == null)
                return false;
            Template template = OtherFunctions.TemplateModelToTemplate(model.TemplateModel, context);
            if (string.IsNullOrEmpty(template.TemplatePath))
                template.TemplatePath = $"~/Views/Templates/";
            if (template != null)
                template.ID = model.itemID.Value;
            if (!Validator.TryValidateObject(template, new ValidationContext(template), null))
                return false;
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            Directory.CreateDirectory($"{env.ContentRootPath}/Views/Templates");
            OtherFunctions.SetUniqueTemplateName(db, template);
            template.TemplatePath += $"{template.Name}.cshtml";
            if (!changeTemplate.TemplatePath.Equals(template.TemplatePath))
            {
                File.Delete($"{env.ContentRootPath}/Views/Templates/{changeTemplate.Name}.cshtml");
            }
            using (StreamWriter sw = new StreamWriter($"{env.ContentRootPath}/Views/Templates/{template.Name}.cshtml"))
            {
                sw.Write(OtherFunctions.SourceToCSHTML(template.TemplateSource));
            }
            db.Entry(changeTemplate).State = EntityState.Detached;
            db.Templates.Update(template);
            db.SaveChanges();
            return true;
        }
    }
}
