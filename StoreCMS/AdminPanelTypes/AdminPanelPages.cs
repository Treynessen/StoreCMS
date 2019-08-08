namespace Treynessen.AdminPanelTypes
{
    public enum AdminPanelPages
    {
        MainPage = 1,
        Pages,
        Categories,
        Redirections,
        Templates,
        Chunks,
        FileManager,
        Users,
        UserTypes,
        SynonymsForStrings, // Для поиска товара
        UserProfile,
        Settings,

        AddPage,
        EditPage,
        DeletePage,

        AddCategory,
        DeleteCategory,
        EditCategory,
        CategoryProducts,
        AddProduct,
        EditProduct,
        DeleteProduct,

        ProductImages,
        AddProductImage,
        DeleteProductImage,

        AddRedirection,
        EditRedirection,
        DeleteRedirection,

        AddTemplate,
        EditTemplate,
        DeleteTemplate,

        AddChunk,
        EditChunk,
        DeleteChunk,

        UploadFile,
        CreateFolder,
        CreateStyle,
        CreateScript,
        EditStyle,
        EditScript,
        DeleteFileOrFolder,
        
        AddUser,
        EditUser,
        DeleteUser,
        UserActions,

        AddUserType,
        EditUserType,
        DeleteUserType,

        AddSynonymForString,
        EditSynonymForString,
        DeleteSynonymForString,

        EditUserData,

        EditSettings,

        LoginForm,
        GetVisitors,
        GetVisitorActions,
        GetUserLog
    }
}