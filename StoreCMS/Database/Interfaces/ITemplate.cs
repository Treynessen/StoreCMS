namespace Treynessen.Database.Interfaces
{
    public interface ITemplate : IKeyID
    {
        string Name { get; set; }
        string TemplateSource { get; set; }
        string TemplatePath { get; set; }
    }
}