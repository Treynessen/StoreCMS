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
            configContentBuilder.Append($"\t\t\"NumberOfProductsOnPage\": \"{(model.NumberOfProductsOnPage.HasValue ? model.NumberOfProductsOnPage.Value : 1)}\",\n");
            configContentBuilder.Append($"\t\t\"PaginationStyleName\": \"{model.PaginationStyleName}\"\n" + "\t},\n");

            configContentBuilder.Append("\t\"AdminPanelAccessSettings\": {\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.MainPage}\": \"{(model.AccessSettings.MainPage.HasValue ? (int)model.AccessSettings.MainPage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Pages}\": \"{(model.AccessSettings.Pages.HasValue ? (int)model.AccessSettings.Pages.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddPage}\": \"{(model.AccessSettings.AddPage.HasValue ? (int)model.AccessSettings.AddPage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditPage}\": \"{(model.AccessSettings.EditPage.HasValue ? (int)model.AccessSettings.EditPage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeletePage}\": \"{(model.AccessSettings.DeletePage.HasValue ? (int)model.AccessSettings.DeletePage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.Categories}\": \"{(model.AccessSettings.Categories.HasValue ? (int)model.AccessSettings.Categories.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.CategoryProducts}\": \"{(model.AccessSettings.CategoryProducts.HasValue ? (int)model.AccessSettings.CategoryProducts.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddProduct}\": \"{(model.AccessSettings.AddProduct.HasValue ? (int)model.AccessSettings.AddProduct.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.EditProduct}\": \"{(model.AccessSettings.EditProduct.HasValue ? (int)model.AccessSettings.EditProduct.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteProduct}\": \"{(model.AccessSettings.DeleteProduct.HasValue ? (int)model.AccessSettings.DeleteProduct.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.ProductImages}\": \"{(model.AccessSettings.ProductImages.HasValue ? (int)model.AccessSettings.ProductImages.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.AddProductImage}\": \"{(model.AccessSettings.AddProductImage.HasValue ? (int)model.AccessSettings.AddProductImage.Value : (int)AccessLevel.VeryHigh)}\",\n");
            configContentBuilder.Append($"\t\t\"{AdminPanelPages.DeleteProductImage}\": \"{(model.AccessSettings.DeleteProductImage.HasValue ? (int)model.AccessSettings.DeleteProductImage.Value : (int)AccessLevel.VeryHigh)}\",\n");
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