namespace Treynessen.Localization
{
    public interface IFileManagerLocalization
    {
        string ChooseTheFile { get; }
        string FolderName { get; }
        string CssFileName { get; }
        string Create { get; }
        string Name { get; }
        string Delete { get; }
        string Folder { get; }
        string Image { get; }
        string Style { get; }

        string IncorrectInput { get; }
        string StyleFileContent { get; }
        string SaveButton { get; }
    }
}