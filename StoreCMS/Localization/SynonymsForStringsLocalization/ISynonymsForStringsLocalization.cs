namespace Treynessen.Localization
{
    public interface ISynonymsForStringsLocalization
    {
        string PageName { get; }

        string String { get; }
        string Synonym { get; }

        string Add { get; }
        string Edit { get; }
        string Delete { get; }

        string RequestCannotBeCompleted { get; }

        string SynonymForStringAdded { get; }
        string SynonymForStringEdited { get; }
        string SynonymForStringDeleted { get; }
    }
}