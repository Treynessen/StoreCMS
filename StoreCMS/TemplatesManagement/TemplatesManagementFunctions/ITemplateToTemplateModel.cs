using Treynessen.AdminPanelTypes;
using Treynessen.Database.Interfaces;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        public static TemplateModel ITemplateToTemplateModel(ITemplate template)
        {
            if (template == null)
                return null;
            TemplateModel model = new TemplateModel
            {
                ID = template.ID,
                Name = template.Name,
                TemplateSource = template.TemplateSource,
                TemplatePath = template.TemplatePath
            };
            return model;
        }
    }
}
