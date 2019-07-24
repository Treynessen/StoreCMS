namespace Treynessen.Localization
{
    public interface IRedirectionsLocalization
    {
        string PageName { get; }

        string Link { get; }
        string Redirection { get; }

        string Add { get; }
        string Edit { get; }
        string Delete { get; }

        string RequestCannotBeCompleted { get; }

        string RedirectionAdded { get; }
        string RedirectionEdited { get; }
        string RedirectionDeleted { get; }
    }
}