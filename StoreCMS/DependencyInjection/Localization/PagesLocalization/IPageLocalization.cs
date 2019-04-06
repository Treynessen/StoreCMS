namespace Trane.DependencyInjection
{
    public interface IPageLocalization
    {
        string AddPage { get; }

        string Title { get; }
        string BreadcrumbName { get; }
        string IncorrectInput { get; }
    }
}