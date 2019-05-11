using Microsoft.AspNetCore.Http;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Interfaces;

namespace Treynessen.Functions
{
    public static partial class OtherFunctions
    {
        public static T TemplateModelToITemplate<T>(TemplateModel model, HttpContext context) where T : class, ITemplate, new()
        {
            if (model == null)
                return null;
            T template = new T();
            template.Name = GetCorrectName(model.Name, context);
            if (string.IsNullOrEmpty(template.Name))
                return null;
            template.TemplatePath = model.TemplatePath;
            template.TemplateSource = model.TemplateSource;
            return template;
        }
    }
}
