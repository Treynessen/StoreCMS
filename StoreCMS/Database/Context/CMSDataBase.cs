using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.Database.Context
{
    public class CMSDatabase : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<SecurityInstallation> SecurityInstallation { get; set; }

        public DbSet<ConnectedUser> ConnectedUsers { get; set; }

        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<VisitedPage> VisitedPages { get; set; }

        public DbSet<UsualPage> UsualPages { get; set; }
        public DbSet<CategoryPage> CategoryPages { get; set; }
        public DbSet<ProductPage> ProductPages { get; set; }
        
        public DbSet<Template> Templates { get; set; }
        public DbSet<Chunk> Chunks { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<Redirection> Redirections { get; set; }

        public DbSet<SynonymForString> SynonymsForStrings { get; set; }

        public DbSet<AdminPanelLog> AdminPanelLogs { get; set; }

        public CMSDatabase(DbContextOptions<CMSDatabase> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            PostgreSqlModel.Configure(modelBuilder);
        }
    }
}
