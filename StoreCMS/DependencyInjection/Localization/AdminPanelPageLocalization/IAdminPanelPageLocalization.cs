namespace Trane.DependencyInjection
{
    public interface IAdminPanelPageLocalization
    {
        string Title { get; }

        string MainPage { get; }
        string Pages { get; }
        string Settings { get; }
    }
}