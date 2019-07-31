namespace Treynessen.Localization
{
    public interface ITemplatesLocalization
    {
        string TemplatesPageName { get; }
        string AddTemplatePageName { get; }
        string EditTemplatePageName { get; }
        string ChunksPageName { get; }
        string AddChunkPageName { get; }
        string EditChunkPageName { get; }

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

        string TemplateCreated { get; }
        string TemplateEdited { get; }
        string TemplateDeleted { get; }
        string ChunkCreated { get; }
        string ChunkEdited { get; }
        string ChunkDeleted { get; }

        string IncorrectInput { get; }
        string TemplateNotFound { get; }
    }
}