namespace Treynessen.Localization
{
    public interface IMainPageLocalization
    {
        string PageName { get; }

        string Welcome { get; }
        string SiteVisitsToday { get; }
        string GetVisitorList { get; }
    }
}