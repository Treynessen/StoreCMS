namespace Treynessen.Localization
{
    public interface IFileManagerLocalization
    {
        string FileManagerPageName { get; }
        string EditStylePageName { get; }
        string EditScriptPageName { get; }

        string ChooseTheFile { get; }
        string FolderName { get; }
        string CssFileName { get; }
        string ScriptFileName { get; }
        string Create { get; }
        string Name { get; }
        string Delete { get; }
        string Folder { get; }
        string Image { get; }
        string Style { get; }
        string Script { get; }

        string IncorrectInput { get; }
        string FileContent { get; }
        string SaveButton { get; }
    }
}