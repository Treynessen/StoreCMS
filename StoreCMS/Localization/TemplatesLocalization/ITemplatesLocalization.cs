namespace Treynessen.Localization
{
    public interface ITemplatesLocalization
    {
        string AddTemplate { get; }
        string AddTemplateChunk { get; }
        string EditTemplate { get; }
        string EditTemplateChunk { get; }
        string DeleteTemplate { get; }
        string DeleteTemplateChunk { get; }
        string Name { get; }
        string TemplateName { get; }
        string TemplateChunkName { get; }
        string Actions { get; }

        string SaveButton { get; }
        string TemplateCode { get; }

        string IncorrectInput { get; }
    }
}