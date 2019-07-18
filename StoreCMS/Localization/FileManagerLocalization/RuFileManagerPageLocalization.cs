namespace Treynessen.Localization
{
    public class RuFileManagerLocalization : IFileManagerLocalization
    {
        public string FileManagerPageName => "Файловый менеджер";
        public string EditStylePageName => "Редактирование стиля";
        public string EditScriptPageName => "Редактирование скрипта";

        public string UploadFile => "Загрузить файл";
        public string FolderName => "Название папки";
        public string CssFileName => "Название .css файла";
        public string ScriptFileName => "Название .js файла";
        public string Create => "Создать";
        public string Name => "Название";
        public string Delete => "Удалить";
        public string Folder => "Папка";
        public string Image => "Изображение";
        public string Style => "Стиль";
        public string Script => "Скрипт";

        public string IncorrectInput => "Обязательные поля не заполнены или содержат недопустимые символы";
        public string FileContent => "Содержимое файла";
        public string SaveButton => "Сохранить";

        public string FolderCreated => "Папка создана";
        public string StyleFileCreated => "Файл .css создан";
        public string ScriptFileCreated => "Файл .js создан";
        public string Deleted => "Удалено";
        public string IncorrectRequest => "Неверный запрос";
        public string FileUploaded => "Файл загружен";
        public string UnsuccessfulFileUpload => "Ошибка при загрузке файла";
        public string StyleFileEdited => "Файл .css изменен";
        public string ScriptFileEdited => "Файл .js изменен";
    }
}