namespace Treynessen.Localization
{
    public class RuSynonymsForStringsLocalization : ISynonymsForStringsLocalization
    {
        public string PageName => "Синонимы к строкам";

        public string String => "Строка";
        public string Synonym => "Синоним";

        public string Add => "Добавить";
        public string Edit => "Изменить";
        public string Delete => "Удалить";

        public string RequestCannotBeCompleted => "Запрос не может быть выполнен";

        public string SynonymForStringAdded => "Синоним для строки добавлен";
        public string SynonymForStringEdited => "Синоним для строки изменен";
        public string SynonymForStringDeleted => "Синоним для строки удален";
    }
}