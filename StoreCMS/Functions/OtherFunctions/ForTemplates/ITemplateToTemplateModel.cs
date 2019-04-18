using Treynessen.AdminPanelTypes;
using Treynessen.Database.Interfaces;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static TemplateModel ITemplateToTemplateModel(ITemplate template)
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