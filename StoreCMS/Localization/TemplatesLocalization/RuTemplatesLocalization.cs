namespace Treynessen.Localization
{
    public class RuTemplatesLocalization : ITemplatesLocalization
    {
        public string TemplatesPageName => "Шаблоны";
        public string AddTemplatePageName => "Создание шаблона";
        public string EditTemplatePageName => "Редактирование шаблона";
        public string ChunksPageName => "Части шаблонов";
        public string AddChunkPageName => "Создание части шаблона";
        public string EditChunkPageName => "Редактирование части шаблона";

        public string AddTemplate => "Добавить шаблон";
        public string AddChunk => "Добавить часть шаблона";
        public string EditTemplate => "Изменить шаблон";
        public string EditChunk => "Изменить часть шаблона";
        public string DeleteTemplate => "Удалить шаблон";
        public string DeleteChunk => "Удалить часть шаблона";
        public string Name => "Название";
        public string TemplateName => "Название шаблона";
        public string ChunkName => "Название части шаблона";
        public string Actions => "Действия";

        public string SaveButton => "Сохранить";
        public string TemplateCode => "Код шаблона";

        public string TemplateCreated => "Шаблон создан";
        public string TemplateEdited => "Шаблон изменен";
        public string TemplateDeleted => "Шаблон удален";
        public string ChunkCreated => "Часть шаблона создана";
        public string ChunkEdited => "Часть шаблона изменена";
        public string ChunkDeleted => "Часть шаблона удалена";

        public string IncorrectInput => "Обязательные поля не заполнены или содержат недопустимые символы";
    }
}