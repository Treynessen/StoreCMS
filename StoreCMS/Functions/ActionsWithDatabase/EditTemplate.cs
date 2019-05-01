using System.IO;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Treynessen.Extensions;
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
            db.Entry(changeTemplate).State = EntityState.Detached;
            if (changeTemplate == null)
                return false;
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            Template template = OtherFunctions.TemplateModelToITemplate<Template>(model.TemplateModel, context);
            if (template != null)
            {
                if (string.IsNullOrEmpty(template.TemplatePath))
                    template.TemplatePath = env.GetTemplatesPath(true);
                template.ID = model.itemID.Value;
            }
            if (!Validator.TryValidateObject(template, new ValidationContext(template), null))
                return false;

            string pathToTemplates = env.GetTemplatesPath();
            if (template.Name.Equals("_ViewImports", System.StringComparison.CurrentCultureIgnoreCase))
                template.Name = "view_imports";
            OtherFunctions.SetUniqueITemplateName(db, template);
            template.TemplatePath += $"{template.Name}.cshtml";

            bool isChangedName = !changeTemplate.Name.Equals(template.Name);
            bool isChangedSource = string.IsNullOrEmpty(changeTemplate.TemplateSource) ? 
                !string.IsNullOrEmpty(template.TemplateSource) :
                !changeTemplate.TemplateSource.Equals(template.TemplateSource);
            if (isChangedName && !isChangedSource)
            {
                try
                {
                    File.Move($"{pathToTemplates}{changeTemplate.Name}.cshtml", $"{pathToTemplates}{template.Name}.cshtml");
                }
                catch (FileNotFoundException)
                {
                    OtherFunctions.SourceToCSHTML(db, pathToTemplates, template.Name, template.TemplateSource);
                }
            }
            else if (isChangedSource)
            {
                File.Delete($"{pathToTemplates}{changeTemplate.Name}.cshtml");
                OtherFunctions.SourceToCSHTML(db, pathToTemplates, template.Name, template.TemplateSource);
            }

            db.Templates.Update(template);
            db.SaveChanges();
            return true;
        }
    }
}
