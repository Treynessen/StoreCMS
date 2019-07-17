namespace Treynessen.Localization
{
    public interface IPagesLocalization
    {
        string Pages_PageName { get; }
        string AddPage_PageName { get; }
        string EditPage_PageName { get; }

        string AddPage { get; }
        string Name { get; }
        string ActionsWithPage { get; }
        string EditPage { get; }
        string DeletePage { get; }

        string SaveButton { get; }
        string Title { get; }
        string Breadcrumb { get; }
        string PageDescription { get; }
        string PageKeywords { get; }
        string Template { get; }
        string WithoutTemplate { get; }
        string PreviousPage { get; }
        string WithoutPreviousPage { get; }
        string Alias { get; }
        string IsMainPage { get; }
        string Published { get; }
        string IsIndex { get; }
        string IsFollow { get; }
        string Content { get; }

        string PageAdded { get; }
        string PageEdited { get; }
        string PageDeleted { get; }
        string IncorrectInput { get; }
    }
}