using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Entities;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static TemplateModel TemplateToTemplateModel(Template template)
        {
            if (template == null)
                return null;
            TemplateModel model = new TemplateModel
            {
                Name = template.Name,
                TemplateSource = template.TemplateSource,
                TemplatePath = template.TemplatePath
            };
            return model;
        }
    }
}
