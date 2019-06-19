﻿namespace Treynessen.Localization
{
    public interface ISettingsLocalization
    {
        string AccessLevelSettings { get; }
        string AccessLevelToAdminPanel { get; }
        string AccessLevelToPages { get; }
        string AccessLevelToAddPage { get; }
        string AccessLevelToEditPage { get; }
        string AccessLevelToDeletePage { get; }
        string AccessLevelToCategories { get; }
        string AccessLevelToCategoryProducts { get; }
        string AccessLevelToAddProduct { get; }
        string AccessLevelToEditProduct { get; }
        string AccessLevelToDeleteProduct { get; }
        string AccessLevelToProductImages { get; }
        string AccessLevelToAddProductImage { get; }
        string AccessLevelToDeleteProductImage { get; }
        string AccessLevelToTemplates { get; }
        string AccessLevelToAddTemplate { get; }
        string AccessLevelToEditTemplate { get; }
        string AccessLevelToDeleteTemplate { get; }
        string AccessLevelToChunks { get; }
        string AccessLevelToAddChunk { get; }
        string AccessLevelToEditChunk { get; }
        string AccessLevelToDeleteChunk { get; }
        string AccessLevelToFiles { get; }
        string AccessLevelToUploadFile { get; }
        string AccessLevelToCreateFolder { get; }
        string AccessLevelToCreateStyle { get; }
        string AccessLevelToEditStyle { get; }
        string AccessLevelToDeleteFileOrFolder { get; }
        string AccessLevelToSettings { get; }
        string AccessLevelToEditSettings { get; }

        string VeryLowAccessLevel { get; }
        string LowAccessLevel { get; }
        string MiddleAccessLevel { get; }
        string HighAccessLevel { get; }
        string VeryHighAccessLevel { get; }
    }
}