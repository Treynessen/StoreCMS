namespace Treynessen.Database.Interfaces
{
    public interface ITemplate
    {
        int ID { get; set; }
        string Name { get; set; }
        string TemplateSource { get; set; }
        string TemplatePath { get; set; }
    }
}