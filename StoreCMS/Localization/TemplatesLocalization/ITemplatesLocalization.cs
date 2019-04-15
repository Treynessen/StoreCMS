namespace Treynessen.Localization
{
    public interface ITemplatesLocalization
    {
        string AddTemplate { get; }
        string EditTemplate { get; }
        string DeleteTemplate { get; }
        string Name { get; }
        string Actions { get; }

        string SaveButton { get; }
        string TemplateCode { get; }

        string IncorrectInput { get; }
    }
}