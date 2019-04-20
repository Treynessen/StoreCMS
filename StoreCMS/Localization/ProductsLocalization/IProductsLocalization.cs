namespace Treynessen.Localization
{
    public interface IProductsLocalization
    {
        string Name { get; }
        string AddProductInCategory { get; }
        string ActionsWithProduct { get; }
        string EditProduct { get; }
        string DeleteProduct { get; }

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
        string IsRobotIndex { get; }
        string Content { get; }
    }
}
