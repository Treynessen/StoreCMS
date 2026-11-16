using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Treynessen.Extensions;
using Treynessen.Localization;
using Treynessen.LogManagement;
using Treynessen.AdminPanelTypes;
using Treynessen.Database.Context;
using Treynessen.SettingsManagement;
using Treynessen.TemplatesManagement;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController : Controller
    {
        [NonAction]
        public IActionResult EditSettings(CMSDatabase db, SettingsModel model, HttpContext context)
        {
            ConfigurationHandler configurationHandler = context.RequestServices.GetRequiredService<ConfigurationHandler>();
            lock (ConfigurationHandler.WriteLock)
            {
                IWebHostEnvironment env = null;
                string cshtmlTemplate = null;
                if (!string.IsNullOrEmpty(model.ProductBlockTemplate))
                {
                    env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                    string[] additions = {
                        "@using Treynessen.Functions;",
                        "@using Treynessen.Database.Entities;",
                        "@addTagHelper Treynessen.TagHelpers.ImageTagHelper, StoreCMS"
                    };
                    cshtmlTemplate = TemplatesManagementFunctions.SourceToCSHTML(
                        db: db,
                        source: model.ProductBlockTemplate,
                        modelType: "ProductPage",
                        env: env,
                        skipChunkName: null,
                        additions: additions
                    );
                }
                // [Комментарий ИИ-агента]: Оба текста подготовлены до первой публикации; повторное сохранение также восстанавливает представление после частичного отказа.
                configurationHandler.ChangeConfigFile(model);
                if (cshtmlTemplate != null)
                    PublishProductBlock(env.GetProductBlockTemplateFullPath(), env.GetProductBlockCshtmlFullPath(),
                        model.ProductBlockTemplate, cshtmlTemplate);
            }
            LogManagementFunctions.AddAdminPanelLog(
                db: db,
                context: context,
                info: (context.Items["LogLocalization"] as IAdminPanelLogLocalization)?.SettingsEdited
            );
            return StatusCode(200);
        }

        internal static void PublishProductBlock(string sourcePath, string viewPath, string source, string view)
        {
            TemplatesManagementFunctions.WriteCompleteTextFile(sourcePath, source);
            try { TemplatesManagementFunctions.WriteCompleteTextFile(viewPath, view); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                // [Комментарий ИИ-агента]: Пара не атомарна: исходник уже опубликован, прежнее представление не усекается; автоматического отката или повтора нет.
                throw new IOException("StoreCMS.ProductBlock.PartiallyPublished: исходник сохранён, публикация представления не подтверждена.", error);
            }
        }
    }
}
