namespace Treynessen.Localization
{
    public interface IAdminPanelPageLocalization
    {
        string Title { get; }

        string MainPage { get; }
        string Pages { get; }
        string CategoriesAndProducts { get; }
        string Templates { get; }
        string Chunks { get; }
        string FileManager { get; }
        string SynonymsForStrings { get; }
        string Settings { get; }
    }
}