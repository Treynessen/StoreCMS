namespace Trane.Localization
{
    public interface ILocalization
    {
        // ДЛЯ СТРАНИЦЫ 404
        string PageNotFound { get; }

        // ДЛЯ ПАНЕЛИ АДМИНИСТРАТОРА
        string AdminPanelTitle { get; }
    }
}
