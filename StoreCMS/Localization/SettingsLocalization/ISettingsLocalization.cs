namespace Treynessen.Localization
{
    public interface ISettingsLocalization
    {
        string PageName { get; }

        string AccessLevelSettings { get; }
        string AccessLevelToAdminPanel { get; }
        string AccessLevelToGetVisitors { get; }
        string AccessLevelToGetVisitorActions { get; }
        string AccessLevelToPages { get; }
        string AccessLevelToAddPage { get; }
        string AccessLevelToEditPage { get; }
        string AccessLevelToDeletePage { get; }
        string AccessLevelToCategories { get; }
        string AccessLevelToAddCategory { get; }
        string AccessLevelToEditCategory { get; }
        string AccessLevelToDeleteCategory { get; }
        string AccessLevelToCategoryProducts { get; }
        string AccessLevelToAddProduct { get; }
        string AccessLevelToEditProduct { get; }
        string AccessLevelToDeleteProduct { get; }
        string AccessLevelToProductImages { get; }
        string AccessLevelToAddProductImage { get; }
        string AccessLevelToDeleteProductImage { get; }
        string AccessLevelToRedirections { get; }
        string AccessLevelToAddRedirection { get; }
        string AccessLevelToEditRedirection { get; }
        string AccessLevelToDeleteRedirection { get; }
        string AccessLevelToTemplates { get; }
        string AccessLevelToAddTemplate { get; }
        string AccessLevelToEditTemplate { get; }
        string AccessLevelToDeleteTemplate { get; }
        string AccessLevelToChunks { get; }
        string AccessLevelToAddChunk { get; }
        string AccessLevelToEditChunk { get; }
        string AccessLevelToDeleteChunk { get; }
        string AccessLevelToFileManager { get; }
        string AccessLevelToUploadFile { get; }
        string AccessLevelToCreateFolder { get; }
        string AccessLevelToCreateStyle { get; }
        string AccessLevelToCreateScript { get; }
        string AccessLevelToEditStyle { get; }
        string AccessLevelToEditScript { get; }
        string AccessLevelToDeleteFileOrFolder { get; }
        string AccessLevelToUsers { get; }
        string AccessLevelToAddUser { get; }
        string AccessLevelToEditUser { get; }
        string AccessLevelToDeleteUser { get; }
        string AccessLevelToUserActions { get; }
        string AccessLevelToUserTypes { get; }
        string AccessLevelToAddUserType { get; }
        string AccessLevelToEditUserType { get; }
        string AccessLevelToDeleteUserType { get; }
        string AccessLevelToSynonymsForStrings { get; }
        string AccessLevelToAddSynonymForString { get; }
        string AccessLevelToEditSynonymForString { get; }
        string AccessLevelToDeleteSynonymForString { get; }
        string AccessLevelToUserProfile { get; }
        string AccessLevelToEditUserData { get; }
        string AccessLevelToGetUserLog { get; }
        string AccessLevelToSettings { get; }
        string AccessLevelToEditSettings { get; }

        string VeryLowAccessLevel { get; }
        string LowAccessLevel { get; }
        string MiddleAccessLevel { get; }
        string HighAccessLevel { get; }
        string VeryHighAccessLevel { get; }

        string CategoryPageSettings { get; }
        string NumberOfProductsOnPage { get; }
        string PaginationStyleName { get; }

        string ProductTemplate { get; }
        
        string PageNotFoundSettings { get; }

        string SearchPageSettings { get; }
        string MaxNumberOfSymbolsInSearchQuery { get; }

        string SearchPageTemplate { get; }
        string PageNotFoundTemplate { get; }
        string WithoutTemplate { get; }

        string BDConnectionSettings { get; }
        string ConnectionString { get; }

        string ForcedGarbageCollection { get; }
        string ValueToRun { get; }
        string InputZeroToDisable { get; }
        string ValueInMegabytes { get; }

        string SaveButton { get; }

        string SettingsSaved { get; }
    }
}