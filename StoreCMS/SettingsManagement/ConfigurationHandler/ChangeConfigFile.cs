using System.IO;
using System.Text;
using Treynessen.Security;
using Treynessen.AdminPanelTypes;

namespace Treynessen.SettingsManagement
{
    public partial class ConfigurationHandler
    {
        public void ChangeConfigFile(SettingsModel model)
        {
            StringBuilder configContentBuilder = new StringBuilder();
            configContentBuilder.Append("{\n\t\"DbSettings\": {\n\t\t\"ConnectionString\": ");
            configContentBuilder.Append($"\"{model.DbConnectionString}\"\n" + "\t},\n");

            configContentBuilder.Append("\t\"CategoryPageSettings\": {\n");
            configContentBuilder.Append($"\t\t\"NumberOfProductsOnPage\": \"{(model.NumberOfProductsOnPage ?? 1)}\",\n");
            configContentBuilder.Append($"\t\t\"PaginationStyleName\": \"{model.PaginationStyleName}\"\n" + "\t},\n");

            configContentBuilder.Append("\t\"SearchPageSettings\": {\n");
            configContentBuilder.Append($"\t\t\"SearchPageTemplateId\": \"{(model.SearchPageTemplateId ?? 0)}\",\n");
            configContentBuilder.Append($"\t\t\"MaxNumberOfSymbolsInSearchQuery\": \"{(model.MaxNumberOfSymbolsInSearchQuery ?? 0)}\"\n" + "\t},\n");

            configContentBuilder.Append("\t\"PageNotFoundSettings\": {\n");
            configContentBuilder.Append($"\t\t\"PageNotFoundTemplateId\": \"{(model.PageNotFoundTemplateId ?? 0)}\"\n" + "\t},\n");

            configContentBuilder.Append("\t\"AdminPanelAccessSettings\": {\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.MainPage}\": \"{(model.AccessSettings.MainPage.HasValue ? (int)model.AccessSettings.MainPage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Pages}\": \"{(model.AccessSettings.Pages.HasValue ? (int)model.AccessSettings.Pages.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddPage}\": \"{(model.AccessSettings.AddPage.HasValue ? (int)model.AccessSettings.AddPage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditPage}\": \"{(model.AccessSettings.EditPage.HasValue ? (int)model.AccessSettings.EditPage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeletePage}\": \"{(model.AccessSettings.DeletePage.HasValue ? (int)model.AccessSettings.DeletePage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Categories}\": \"{(model.AccessSettings.Categories.HasValue ? (int)model.AccessSettings.Categories.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddCategory}\": \"{(model.AccessSettings.AddCategory.HasValue ? (int)model.AccessSettings.AddCategory.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditCategory}\": \"{(model.AccessSettings.EditCategory.HasValue ? (int)model.AccessSettings.EditCategory.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteCategory}\": \"{(model.AccessSettings.DeleteCategory.HasValue ? (int)model.AccessSettings.DeleteCategory.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.CategoryProducts}\": \"{(model.AccessSettings.CategoryProducts.HasValue ? (int)model.AccessSettings.CategoryProducts.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddProduct}\": \"{(model.AccessSettings.AddProduct.HasValue ? (int)model.AccessSettings.AddProduct.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditProduct}\": \"{(model.AccessSettings.EditProduct.HasValue ? (int)model.AccessSettings.EditProduct.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteProduct}\": \"{(model.AccessSettings.DeleteProduct.HasValue ? (int)model.AccessSettings.DeleteProduct.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.ProductImages}\": \"{(model.AccessSettings.ProductImages.HasValue ? (int)model.AccessSettings.ProductImages.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddProductImage}\": \"{(model.AccessSettings.AddProductImage.HasValue ? (int)model.AccessSettings.AddProductImage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteProductImage}\": \"{(model.AccessSettings.DeleteProductImage.HasValue ? (int)model.AccessSettings.DeleteProductImage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Redirections}\": \"{(model.AccessSettings.Redirections.HasValue ? (int)model.AccessSettings.Redirections.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddRedirection}\": \"{(model.AccessSettings.AddRedirection.HasValue ? (int)model.AccessSettings.AddRedirection.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditRedirection}\": \"{(model.AccessSettings.EditRedirection.HasValue ? (int)model.AccessSettings.EditRedirection.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteRedirection}\": \"{(model.AccessSettings.DeleteRedirection.HasValue ? (int)model.AccessSettings.DeleteRedirection.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Templates}\": \"{(model.AccessSettings.Templates.HasValue ? (int)model.AccessSettings.Templates.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddTemplate}\": \"{(model.AccessSettings.AddTemplate.HasValue ? (int)model.AccessSettings.AddTemplate.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditTemplate}\": \"{(model.AccessSettings.EditTemplate.HasValue ? (int)model.AccessSettings.EditTemplate.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteTemplate}\": \"{(model.AccessSettings.DeleteTemplate.HasValue ? (int)model.AccessSettings.DeleteTemplate.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Chunks}\": \"{(model.AccessSettings.Chunks.HasValue ? (int)model.AccessSettings.Chunks.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddChunk}\": \"{(model.AccessSettings.AddChunk.HasValue ? (int)model.AccessSettings.AddChunk.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditChunk}\": \"{(model.AccessSettings.EditChunk.HasValue ? (int)model.AccessSettings.EditChunk.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteChunk}\": \"{(model.AccessSettings.DeleteChunk.HasValue ? (int)model.AccessSettings.DeleteChunk.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.FileManager}\": \"{(model.AccessSettings.FileManager.HasValue ? (int)model.AccessSettings.FileManager.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.UploadFile}\": \"{(model.AccessSettings.UploadFile.HasValue ? (int)model.AccessSettings.UploadFile.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.CreateFolder}\": \"{(model.AccessSettings.CreateFolder.HasValue ? (int)model.AccessSettings.CreateFolder.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.CreateStyle}\": \"{(model.AccessSettings.CreateStyle.HasValue ? (int)model.AccessSettings.CreateStyle.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.CreateScript}\": \"{(model.AccessSettings.CreateScript.HasValue ? (int)model.AccessSettings.CreateScript.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditStyle}\": \"{(model.AccessSettings.EditStyle.HasValue ? (int)model.AccessSettings.EditStyle.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditScript}\": \"{(model.AccessSettings.EditScript.HasValue ? (int)model.AccessSettings.EditScript.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteFileOrFolder}\": \"{(model.AccessSettings.DeleteFileOrFolder.HasValue ? (int)model.AccessSettings.DeleteFileOrFolder.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Users}\": \"{(model.AccessSettings.Users.HasValue ? (int)model.AccessSettings.Users.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddUser}\": \"{(model.AccessSettings.AddUser.HasValue ? (int)model.AccessSettings.AddUser.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditUser}\": \"{(model.AccessSettings.EditUser.HasValue ? (int)model.AccessSettings.EditUser.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteUser}\": \"{(model.AccessSettings.DeleteUser.HasValue ? (int)model.AccessSettings.DeleteUser.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.UserTypes}\": \"{(model.AccessSettings.UserTypes.HasValue ? (int)model.AccessSettings.UserTypes.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddUserType}\": \"{(model.AccessSettings.AddUserType.HasValue ? (int)model.AccessSettings.AddUserType.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditUserType}\": \"{(model.AccessSettings.EditUserType.HasValue ? (int)model.AccessSettings.EditUserType.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteUserType}\": \"{(model.AccessSettings.DeleteUserType.HasValue ? (int)model.AccessSettings.DeleteUserType.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.SynonymsForStrings}\": \"{(model.AccessSettings.SynonymsForStrings.HasValue ? (int)model.AccessSettings.SynonymsForStrings.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddSynonymForString}\": \"{(model.AccessSettings.AddSynonymForString.HasValue ? (int)model.AccessSettings.AddSynonymForString.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditSynonymForString}\": \"{(model.AccessSettings.EditSynonymForString.HasValue ? (int)model.AccessSettings.EditSynonymForString.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteSynonymForString}\": \"{(model.AccessSettings.DeleteSynonymForString.HasValue ? (int)model.AccessSettings.DeleteSynonymForString.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.UserProfile}\": \"{(model.AccessSettings.UserProfile.HasValue ? (int)model.AccessSettings.UserProfile.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditUserData}\": \"{(model.AccessSettings.EditUserData.HasValue ? (int)model.AccessSettings.EditUserData.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.GetUserLog}\": \"{(model.AccessSettings.GetUserLog.HasValue ? (int)model.AccessSettings.GetUserLog.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Settings}\": \"{(model.AccessSettings.Settings.HasValue ? (int)model.AccessSettings.Settings.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditSettings}\": \"{(model.AccessSettings.EditSettings.HasValue ? (int)model.AccessSettings.EditSettings.Value : (int)AccessLevel.VeryHigh)}\"\n");
            configContentBuilder.Append("\t}\n}");

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(configContentBuilder.ToString());
            }
        }
    }
}