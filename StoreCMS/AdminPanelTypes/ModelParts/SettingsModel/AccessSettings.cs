using Treynessen.Security;

namespace Treynessen.AdminPanelTypes
{
    public class AccessSettings
    {
        public AccessLevel? MainPage { get; set; }
        public AccessLevel? Pages { get; set; }
        public AccessLevel? Categories { get; set; }
        public AccessLevel? Redirections { get; set; }
        public AccessLevel? Templates { get; set; }
        public AccessLevel? Chunks { get; set; }
        public AccessLevel? FileManager { get; set; }
        public AccessLevel? Users { get; set; }
        public AccessLevel? UserTypes { get; set; }
        public AccessLevel? SynonymsForStrings { get; set; }
        public AccessLevel? UserProfile { get; set; }
        public AccessLevel? Settings { get; set; }

        public AccessLevel? AddPage { get; set; }
        public AccessLevel? EditPage { get; set; }
        public AccessLevel? DeletePage { get; set; }

        public AccessLevel? AddCategory { get; set; }
        public AccessLevel? EditCategory { get; set; }
        public AccessLevel? DeleteCategory { get; set; }

        public AccessLevel? CategoryProducts { get; set; }
        public AccessLevel? AddProduct { get; set; }
        public AccessLevel? EditProduct { get; set; }
        public AccessLevel? DeleteProduct { get; set; }

        public AccessLevel? ProductImages { get; set; }
        public AccessLevel? AddProductImage { get; set; }
        public AccessLevel? DeleteProductImage { get; set; }

        public AccessLevel? AddRedirection { get; set; }
        public AccessLevel? EditRedirection { get; set; }
        public AccessLevel? DeleteRedirection { get; set; }

        public AccessLevel? AddTemplate { get; set; }
        public AccessLevel? EditTemplate { get; set; }
        public AccessLevel? DeleteTemplate { get; set; }

        public AccessLevel? AddChunk { get; set; }
        public AccessLevel? EditChunk { get; set; }
        public AccessLevel? DeleteChunk { get; set; }

        public AccessLevel? UploadFile { get; set; }
        public AccessLevel? CreateFolder { get; set; }
        public AccessLevel? CreateStyle { get; set; }
        public AccessLevel? CreateScript { get; set; }
        public AccessLevel? EditStyle { get; set; }
        public AccessLevel? EditScript { get; set; }
        public AccessLevel? DeleteFileOrFolder { get; set; }

        public AccessLevel? AddUser { get; set; }
        public AccessLevel? EditUser { get; set; }
        public AccessLevel? DeleteUser { get; set; }
        public AccessLevel? UserActions { get; set; }

        public AccessLevel? AddUserType { get; set; }
        public AccessLevel? EditUserType { get; set; }
        public AccessLevel? DeleteUserType { get; set; }

        public AccessLevel? AddSynonymForString { get; set; }
        public AccessLevel? EditSynonymForString { get; set; }
        public AccessLevel? DeleteSynonymForString { get; set; }

        public AccessLevel? EditUserData { get; set; }
        public AccessLevel? GetUserLog { get; set; }

        public AccessLevel? EditSettings { get; set; }
    }
}