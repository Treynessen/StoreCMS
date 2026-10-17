using Treynessen.AdminPanelTypes;

namespace Treynessen.Controllers
{
    public partial class AdminPanelController
    {
        private static bool SupportsAdminPanelMutation(AdminPanelPages? page, string method)
        {
            switch (page)
            {
                case AdminPanelPages.LoginForm:
                case AdminPanelPages.ChangeInitialPassword:
                case AdminPanelPages.AddPage:
                case AdminPanelPages.EditPage:
                case AdminPanelPages.AddCategory:
                case AdminPanelPages.EditCategory:
                case AdminPanelPages.AddProduct:
                case AdminPanelPages.EditProduct:
                case AdminPanelPages.AddRedirection:
                case AdminPanelPages.EditRedirection:
                case AdminPanelPages.AddTemplate:
                case AdminPanelPages.EditTemplate:
                case AdminPanelPages.AddChunk:
                case AdminPanelPages.EditChunk:
                case AdminPanelPages.CreateFolder:
                case AdminPanelPages.CreateStyle:
                case AdminPanelPages.CreateScript:
                case AdminPanelPages.EditStyle:
                case AdminPanelPages.EditScript:
                case AdminPanelPages.AddUser:
                case AdminPanelPages.EditUser:
                case AdminPanelPages.AddUserType:
                case AdminPanelPages.EditUserType:
                case AdminPanelPages.AddSynonymForString:
                case AdminPanelPages.EditSynonymForString:
                case AdminPanelPages.EditUserData:
                case AdminPanelPages.EditSettings:
                    return method == "POST";
                case AdminPanelPages.AddProductImage:
                case AdminPanelPages.UploadFile:
                    return method == "PUT";
                case AdminPanelPages.Exit:
                case AdminPanelPages.DeletePage:
                case AdminPanelPages.DeleteCategory:
                case AdminPanelPages.DeleteProduct:
                case AdminPanelPages.DeleteProductImage:
                case AdminPanelPages.DeleteRedirection:
                case AdminPanelPages.DeleteTemplate:
                case AdminPanelPages.DeleteChunk:
                case AdminPanelPages.DeleteFileOrFolder:
                case AdminPanelPages.DeleteUser:
                case AdminPanelPages.DeleteUserType:
                case AdminPanelPages.DeleteSynonymForString:
                    return method == "DELETE";
                default:
                    return false;
            }
        }
    }
}
