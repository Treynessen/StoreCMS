namespace Treynessen.Localization
{
    public interface IProductsLocalization
    {
        string Name { get; }
        string AddProductInCategory { get; }
        string ActionsWithProduct { get; }
        string EditProduct { get; }
        string ProductImages { get; }
        string DeleteProduct { get; }

        string ChooseTheFile { get; }
        string DeleteProductImage { get; }

        string IncorrectInput { get; }
        string SaveButton { get; }
        string Title { get; }
        string Breadcrumb { get; }
        string Price { get; }
        string OldPrice { get; }
        string ShortDescription { get; }
        string PageDescription { get; }
        string PageKeywords { get; }
        string Template { get; }
        string WithoutTemplate { get; }
        string Alias { get; }
        string Published { get; }
        string IsIndex { get; }
        string IsFollow { get; }
        string Content { get; }
    }
}