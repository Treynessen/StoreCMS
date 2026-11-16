namespace Treynessen.Localization
{
    public interface IAdminPanelLogLocalization
    {
        string LoggedIn { get; }
        string FailedLoginAttempt { get; }

        string PageAdded { get; }
        string PageEdited { get; }
        string PageDeleted { get; }

        string CategoryAdded { get; }
        string CategoryEdited { get; }
        string CategoryDeleted { get; }

        string ProductAdded { get; }
        string ProductEdited { get; }
        string ProductDeleted { get; }
        string ProductImageUploaded { get; }
        string ProductImageDeleted { get; }

        string RedirectionAdded { get; }
        string RedirectionEditedTo { get; }
        string RedirectionDeleted { get; }

        string TemplateAdded { get; }
        string TemplateEdited { get; }
        string TemplateDeleted { get; }

        string ChunkAdded { get; }
        string ChunkEdited { get; }
        string ChunkDeleted { get; }

        string FileUploadedTo { get; }
        string FolderCreatedIn { get; }
        string FileCreatedIn { get; }
        string FileEdited { get; }
        string FileDeleted { get; }
        string FolderDeleted { get; }

        string UserAdded { get; }
        string UserEdited { get; }
        string UserDeleted { get; }

        string UserTypeAdded { get; }
        string UserTypeEdited { get; }
        string UserTypeDeleted { get; }

        string SynonymForStringAdded { get; }
        string SynonymForStringEditedTo { get; }
        string SynonymForStringDeleted { get; }

        string UserDataEdited { get; }

        string SettingsEdited { get; }
    }
}