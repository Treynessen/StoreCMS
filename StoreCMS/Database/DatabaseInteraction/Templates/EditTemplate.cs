using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
        public static void EditTemplate(CMSDatabase db, int? itemID, TemplateModel model, HttpContext context, out bool successfullyCompleted)
        {
            if (!itemID.HasValue || model == null)
            {
                successfullyCompleted = false;
                return;
            }
            Template editableTemplate = db.Templates.AsNoTracking().FirstOrDefault(t => t.ID == itemID);
            if (editableTemplate == null)
            {
                successfullyCompleted = false;
                return;
            }
            Template editedTemplate = TemplatesManagementFunctions.TemplateModelToITemplate<Template>(model, context);
            if (editedTemplate == null)
            {
                successfullyCompleted = false;
                return;
            }
            if (editedTemplate.Name.Equals("_ViewImports", StringComparison.InvariantCultureIgnoreCase))
                editedTemplate.Name = "view_imports";
            IHostingEnvironment env = context.RequestServices.GetService<IHostingEnvironment>();
            editedTemplate.ID = itemID.Value;
            editedTemplate.TemplatePath = env.GetTemplatesFolderShortPath();
            TemplatesManagementFunctions.SetUniqueITemplateName(db, editedTemplate);
            editedTemplate.TemplatePath += $"{editedTemplate.Name}.cshtml";
            db.Templates.Update(editedTemplate);
            db.SaveChanges();
            successfullyCompleted = true;
            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: $"{editableTemplate.Name} (ID-{editableTemplate.ID.ToString()}): {(context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.TemplateEdited}"
            );

            // Изменяем cshtml файл, если изменилось имя шаблона и/или код шаблона
            bool changedName = !editedTemplate.Name.Equals(editableTemplate.Name, StringComparison.InvariantCulture);
            bool changedTemplateSource = !editedTemplate.TemplateSource.Equals(editableTemplate.TemplateSource, StringComparison.InvariantCulture);
            if (changedName && changedTemplateSource)
            {
                string pathToOldFileName = $"{env.GetTemplatesFolderFullPath()}{editableTemplate.Name}.cshtml";
                if (File.Exists(pathToOldFileName))
                    File.Delete(pathToOldFileName);
                string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                    db: db,
                    source: editedTemplate.TemplateSource,
                    modelType: "Page",
                    env: env,
                    skipChunkName: null
                );
                TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetTemplatesFolderFullPath(), editedTemplate.Name, cshtmlContent);
            }
            else if (changedName)
            {
                string pathToOldFileName = $"{env.GetTemplatesFolderFullPath()}{editableTemplate.Name}.cshtml";
                if (File.Exists(pathToOldFileName))
                    File.Move(pathToOldFileName, $"{env.GetTemplatesFolderFullPath()}{editedTemplate.Name}.cshtml");
                else
                {
                    string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                        db: db,
                        source: editedTemplate.TemplateSource,
                        modelType: "Page",
                        env: env,
                        skipChunkName: null
                    );
                    TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetTemplatesFolderFullPath(), editedTemplate.Name, cshtmlContent);
                }
            }
            else if (changedTemplateSource)
            {
                string cshtmlContent = TemplatesManagementFunctions.SourceToCSHTML(
                    db: db,
                    source: editedTemplate.TemplateSource,
                    modelType: "Page",
                    env: env,
                    skipChunkName: null
                );
                TemplatesManagementFunctions.WriteCshtmlContentToFile(env.GetTemplatesFolderFullPath(), editedTemplate.Name, cshtmlContent);
            }
        }
    }
}