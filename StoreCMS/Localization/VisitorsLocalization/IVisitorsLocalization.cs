namespace Treynessen.Localization
{
    public interface IVisitorsLocalization
    {
        string IPAdress { get; }
        string FirstVisit { get; }
        string LastVisit { get; }

        string ActionsOfUser { get; }
        string VisitedUsualPage { get; }
        string VisitedCategoryPage { get; }
        string VisitedProductPage { get; }
    }
}