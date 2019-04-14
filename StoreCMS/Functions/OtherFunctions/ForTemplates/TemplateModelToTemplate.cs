using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static Template TemplateModelToTemplate(TemplateModel model, HttpContext context)
        {
            if (model == null)
                return null;
            Template template = new Template();
            template.Name = GetCorrectName(model.Name, context);
            template.TemplatePath = model.TemplatePath;
            template.TemplateSource = model.TemplateSource;
            return template;
        }
    }
}
