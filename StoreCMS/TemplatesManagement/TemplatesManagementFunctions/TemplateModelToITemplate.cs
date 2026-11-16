using Microsoft.AspNetCore.Http;
using Treynessen.Functions;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Interfaces;

namespace Treynessen.TemplatesManagement
{
    public static partial class TemplatesManagementFunctions
    {
        public static T TemplateModelToITemplate<T>(TemplateModel model, HttpContext context) where T : class, ITemplate, new()
        {
            if (model == null)
                return null;
            T template = new T();
            template.Name = OtherFunctions.GetCorrectName(model.Name, context);
            if (string.IsNullOrEmpty(template.Name))
                return null;
            template.TemplatePath = model.TemplatePath;
            template.TemplateSource = model.TemplateSource == null ? string.Empty : model.TemplateSource;
            return template;
        }
    }
}