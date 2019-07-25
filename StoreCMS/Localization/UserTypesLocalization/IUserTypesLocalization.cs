namespace Treynessen.Localization
{
    public interface IUserTypesLocalization
    {
        string PageName { get; }

        string VeryLowAccessLevel { get; }
        string LowAccessLevel { get; }
        string MiddleAccessLevel { get; }
        string HighAccessLevel { get; }
        string VeryHighAccessLevel { get; }

        string UserTypeName { get; }
        string AccessLevel { get; }

        string Add { get; }
        string Edit { get; }
        string Delete { get; }

        string RequestCannotBeCompleted { get; }

        string UserTypeAdded { get; }
        string UserTypeEdited { get; }
        string UserTypeDeleted { get; }
    }
}