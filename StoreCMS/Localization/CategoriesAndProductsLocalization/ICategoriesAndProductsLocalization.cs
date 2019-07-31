namespace Treynessen.Localization
{
    public interface ICategoriesAndProductsLocalization
    {
        string CategoriesPageName { get; }
        string CategoryProductsPageName { get; }
        string AddCategoryPageName { get; }
        string AddProductPageName { get; }
        string EditCategoryPageName { get; }
        string EditProductPageName { get; }
        string ProductImagesPageName { get; }

        string AddCategory { get; }
        string AddProductInCategory { get; }
        string Name { get; }
        string Actions{ get; }
        string EditCategory { get; }
        string EditProduct { get; }
        string ProductImages { get; }
        string DeleteCategory { get; }
        string DeleteProduct { get; }

        string UploadImage { get; }
        string DeleteProductImage { get; }

        string SaveButton { get; }
        string CategoryTitle { get; }
        string ProductTitle { get; }
        string CategoryBreadcrumb { get; }
        string ProductBreadcrumb { get; }
        string Price { get; }
        string OldPrice { get; }
        string ShortDescription { get; }
        string SpecialProduct { get; }
        string AddParagraphTag { get; }
        string PageDescription { get; }
        string PageKeywords { get; }
        string Template { get; }
        string WithoutTemplate { get; }
        string PreviousPage { get; }
        string WithoutPreviousPage { get; }
        string Alias { get; }
        string Published { get; }
        string IsIndex { get; }
        string IsFollow { get; }
        string Content { get; }

        string CategoryAdded { get; }
        string ProductAdded { get; }
        string CategoryEdited { get; }
        string ProductEdited { get; }
        string CategoryDeleted { get; }
        string ProductDeleted { get; }
        string ImageUploaded { get; }
        string ImageDeleted { get; }
        string CategoryNotFound { get; }
        string ProductNotFound { get; }
        string ImageNotFound { get; }

        string UnsuccessfullyImageUploaded { get; }
        string IncorrectInput { get; }
    }
}