using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class ActionsWithDatabase
    {
        public static bool AddTemplates(CMSDatabase db, TemplateModel model, HttpContext context)
        {
            Template template = OtherFunctions.TemplateModelToTemplate(model, context);
            if (template == null)
                return false;
            if (string.IsNullOrEmpty(template.TemplatePath))
                template.TemplatePath = $"~/Views/Templates/";
            if (!Validator.TryValidateObject(template, new ValidationContext(template), null))
                return false;
            string pathToTemplates = $"{context.RequestServices.GetService<IHostingEnvironment>().ContentRootPath}/Views/Templates";
            OtherFunctions.SetUniqueTemplateName(db, template);
            template.TemplatePath += $"{template.Name}.cshtml";
            OtherFunctions.SourceToCSHTML(pathToTemplates, template.Name, template.TemplateSource);
            template.ID = OtherFunctions.GetDatabaseRawID(db.Templates);
            db.Templates.Add(template);
            db.SaveChanges();
            return true;
        }
    }
}