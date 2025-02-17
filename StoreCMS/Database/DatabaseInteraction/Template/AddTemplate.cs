﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.Database.Entities;
using Treynessen.TemplatesManagement;

namespace Treynessen.Database
{
    public static partial class DatabaseInteraction
    {
        public static void AddTemplate(CMSDatabase db, TemplateModel model, HttpContext context, out bool successfullyCompleted)
        {
            Template template = TemplatesManagementFunctions.TemplateModelToITemplate<Template>(model, context);
            if (template == null)
            {
                successfullyCompleted = false;
                return;
            }
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            if (template.Name.Equals("_ViewImports", System.StringComparison.OrdinalIgnoreCase))
                template.Name = "view_imports";
            template.TemplatePath = env.GetTemplatesFolderShortPath();
            TemplatesManagementFunctions.SetUniqueITemplateName(db, template);
            template.TemplatePath += $"{template.Name}.cshtml";
            string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                db: db,
                source: template.TemplateSource,
                modelType: "Page",
                env: env,
                skipChunkName: null
            );
            TemplatesManagementFunctions.WriteCshtmlContentToFile(
                pathToTemplatesFolder: env.GetTemplatesFolderFullPath(),
                templateName: template.Name,
                chstmlContent: cshtmlContent
            );
            db.Templates.Add(template);
            db.SaveChanges();
            model.ID = template.ID;
            successfullyCompleted = true;

            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{template.Name} (ID-{template.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.TemplateAdded}"
            );
        }
    }
}