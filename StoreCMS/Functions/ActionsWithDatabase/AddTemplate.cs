using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
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
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            if (string.IsNullOrEmpty(template.TemplatePath))
                template.TemplatePath = env.GetTemplatesPath(true);
            if (!Validator.TryValidateObject(template, new ValidationContext(template), null))
                return false;

            if (template.Name.Equals("_ViewImports", System.StringComparison.InvariantCultureIgnoreCase))
                template.Name = "view_imports";
            OtherFunctions.SetUniqueITemplateName(db, template);
            template.TemplatePath += $"{template.Name}.cshtml";
            OtherFunctions.SourceToCSHTML(db, env.GetTemplatesPath(), template.Name, template.TemplateSource);
            template.ID = OtherFunctions.GetDatabaseRawID(db.Templates);
            db.Templates.Add(template);
            db.SaveChanges();
            return true;
        }
    }
}