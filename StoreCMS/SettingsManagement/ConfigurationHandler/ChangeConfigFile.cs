using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Treynessen.Security;
using Treynessen.AdminPanelTypes;
using Treynessen.TemplatesManagement;

namespace Treynessen.SettingsManagement
{
    public partial class ConfigurationHandler
    {
        internal static readonly object WriteLock = new object();

        public void ChangeConfigFile(SettingsModel model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.AccessSettings);
            lock (WriteLock)
            {
                JsonObject root;
                using (var input = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete))
                    root = JsonNode.Parse(input, documentOptions: new JsonDocumentOptions
                    {
                        AllowTrailingCommas = true,
                        CommentHandling = JsonCommentHandling.Skip
                    }) as JsonObject ?? throw new InvalidDataException("Ожидается JSON-объект настроек.");

                // [Комментарий ИИ-агента]: Читаем актуальный документ под общим lock; неизвестные секции и реквизиты БД не пересобираются из устаревшего IConfiguration.
                SetValue(root, "ForcedGarbageCollectionSettings", "ValueToRun", model.ValueToRunForcedGarbageCollection.ToString(CultureInfo.InvariantCulture));
                SetValue(root, "CategoryPageSettings", "NumberOfProductsOnPage", (model.NumberOfProductsOnPage ?? 1).ToString(CultureInfo.InvariantCulture));
                SetValue(root, "CategoryPageSettings", "PaginationStyleName", model.PaginationStyleName ?? "");
                SetValue(root, "SearchPageSettings", "SearchPageTemplateId", (model.SearchPageTemplateId ?? 0).ToString(CultureInfo.InvariantCulture));
                SetValue(root, "SearchPageSettings", "MaxNumberOfSymbolsInSearchQuery", (model.MaxNumberOfSymbolsInSearchQuery ?? 0).ToString(CultureInfo.InvariantCulture));
                SetValue(root, "PageNotFoundSettings", "PageNotFoundTemplateId", (model.PageNotFoundTemplateId ?? 0).ToString(CultureInfo.InvariantCulture));
                foreach ((AdminPanelPages page, AccessLevel? level) in new (AdminPanelPages, AccessLevel?)[]
                {
                        (AdminPanelPages.MainPage, model.AccessSettings.MainPage),
                        (AdminPanelPages.GetVisitors, model.AccessSettings.GetVisitors),
                        (AdminPanelPages.GetVisitorActions, model.AccessSettings.GetVisitorActions),
                        (AdminPanelPages.Pages, model.AccessSettings.Pages),
                        (AdminPanelPages.AddPage, model.AccessSettings.AddPage),
                        (AdminPanelPages.EditPage, model.AccessSettings.EditPage),
                        (AdminPanelPages.DeletePage, model.AccessSettings.DeletePage),
                        (AdminPanelPages.Categories, model.AccessSettings.Categories),
                        (AdminPanelPages.AddCategory, model.AccessSettings.AddCategory),
                        (AdminPanelPages.EditCategory, model.AccessSettings.EditCategory),
                        (AdminPanelPages.DeleteCategory, model.AccessSettings.DeleteCategory),
                        (AdminPanelPages.CategoryProducts, model.AccessSettings.CategoryProducts),
                        (AdminPanelPages.AddProduct, model.AccessSettings.AddProduct),
                        (AdminPanelPages.EditProduct, model.AccessSettings.EditProduct),
                        (AdminPanelPages.DeleteProduct, model.AccessSettings.DeleteProduct),
                        (AdminPanelPages.ProductImages, model.AccessSettings.ProductImages),
                        (AdminPanelPages.AddProductImage, model.AccessSettings.AddProductImage),
                        (AdminPanelPages.DeleteProductImage, model.AccessSettings.DeleteProductImage),
                        (AdminPanelPages.Redirections, model.AccessSettings.Redirections),
                        (AdminPanelPages.AddRedirection, model.AccessSettings.AddRedirection),
                        (AdminPanelPages.EditRedirection, model.AccessSettings.EditRedirection),
                        (AdminPanelPages.DeleteRedirection, model.AccessSettings.DeleteRedirection),
                        (AdminPanelPages.Templates, model.AccessSettings.Templates),
                        (AdminPanelPages.AddTemplate, model.AccessSettings.AddTemplate),
                        (AdminPanelPages.EditTemplate, model.AccessSettings.EditTemplate),
                        (AdminPanelPages.DeleteTemplate, model.AccessSettings.DeleteTemplate),
                        (AdminPanelPages.Chunks, model.AccessSettings.Chunks),
                        (AdminPanelPages.AddChunk, model.AccessSettings.AddChunk),
                        (AdminPanelPages.EditChunk, model.AccessSettings.EditChunk),
                        (AdminPanelPages.DeleteChunk, model.AccessSettings.DeleteChunk),
                        (AdminPanelPages.FileManager, model.AccessSettings.FileManager),
                        (AdminPanelPages.UploadFile, model.AccessSettings.UploadFile),
                        (AdminPanelPages.CreateFolder, model.AccessSettings.CreateFolder),
                        (AdminPanelPages.CreateStyle, model.AccessSettings.CreateStyle),
                        (AdminPanelPages.CreateScript, model.AccessSettings.CreateScript),
                        (AdminPanelPages.EditStyle, model.AccessSettings.EditStyle),
                        (AdminPanelPages.EditScript, model.AccessSettings.EditScript),
                        (AdminPanelPages.DeleteFileOrFolder, model.AccessSettings.DeleteFileOrFolder),
                        (AdminPanelPages.Users, model.AccessSettings.Users),
                        (AdminPanelPages.AddUser, model.AccessSettings.AddUser),
                        (AdminPanelPages.EditUser, model.AccessSettings.EditUser),
                        (AdminPanelPages.DeleteUser, model.AccessSettings.DeleteUser),
                        (AdminPanelPages.UserTypes, model.AccessSettings.UserTypes),
                        (AdminPanelPages.AddUserType, model.AccessSettings.AddUserType),
                        (AdminPanelPages.EditUserType, model.AccessSettings.EditUserType),
                        (AdminPanelPages.DeleteUserType, model.AccessSettings.DeleteUserType),
                        (AdminPanelPages.SynonymsForStrings, model.AccessSettings.SynonymsForStrings),
                        (AdminPanelPages.AddSynonymForString, model.AccessSettings.AddSynonymForString),
                        (AdminPanelPages.EditSynonymForString, model.AccessSettings.EditSynonymForString),
                        (AdminPanelPages.DeleteSynonymForString, model.AccessSettings.DeleteSynonymForString),
                        (AdminPanelPages.UserProfile, model.AccessSettings.UserProfile),
                        (AdminPanelPages.EditUserData, model.AccessSettings.EditUserData),
                        (AdminPanelPages.GetUserLog, model.AccessSettings.GetUserLog),
                        (AdminPanelPages.Settings, model.AccessSettings.Settings),
                        (AdminPanelPages.EditSettings, model.AccessSettings.EditSettings),
                })
                    SetValue(root, "AdminPanelAccessSettings", page.ToString(), ((int)(level ?? AccessLevel.VeryHigh)).ToString(CultureInfo.InvariantCulture));

                string content = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
                TemplatesManagementFunctions.WriteCompleteTextFile(path, content);
            }
        }

        private static string ExistingName(JsonObject node, string name)
        {
            foreach (var property in node)
                if (string.Equals(property.Key, name, StringComparison.OrdinalIgnoreCase)) return property.Key;
            return name;
        }

        private static void SetValue(JsonObject root, string section, string key, string value)
        {
            string sectionName = ExistingName(root, section);
            JsonObject target;
            if (root.TryGetPropertyValue(sectionName, out var node))
                target = node as JsonObject ?? throw new InvalidDataException("Раздел настроек должен быть JSON-объектом.");
            else
                root[sectionName] = target = new JsonObject();
            target[ExistingName(target, key)] = value;
        }
    }
}