namespace Treynessen.Localization
{
    public interface ITemplatesLocalization
    {
        string AddTemplate { get; }
        string AddChunk { get; }
        string EditTemplate { get; }
        string EditChunk { get; }
        string DeleteTemplate { get; }
        string DeleteChunk { get; }
        string Name { get; }
        string TemplateName { get; }
        string ChunkName { get; }
        string Actions { get; }

        string SaveButton { get; }
        string TemplateCode { get; }

        string IncorrectInput { get; }
    }
}