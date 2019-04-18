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
        public static bool AddTemplate(CMSDatabase db, TemplateModel model, HttpContext context)
        {
            Template template = OtherFunctions.TemplateModelToITemplate<Template>(model, context);
            if (template == null)
                return false;
            if (string.IsNullOrEmpty(template.TemplatePath))
                template.TemplatePath = $"~/Views/Templates/";
            if (!Validator.TryValidateObject(template, new ValidationContext(template), null))
                return false;

            string pathToTemplates = $"{context.RequestServices.GetService<IHostingEnvironment>().ContentRootPath}/Views/Templates";
            if (template.Name.Equals("_ViewImports", System.StringComparison.CurrentCultureIgnoreCase))
                template.Name = "view_imports";
            OtherFunctions.SetUniqueITemplateName(db, template);
            template.TemplatePath += $"{template.Name}.cshtml";
            OtherFunctions.SourceToCSHTML(db, pathToTemplates, template.Name, template.TemplateSource);
            template.ID = OtherFunctions.GetDatabaseRawID(db.Templates);
            db.Templates.Add(template);
            db.SaveChanges();
            return true;
        }
    }
}