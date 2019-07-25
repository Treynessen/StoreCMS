using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;

namespace Treynessen.Database.Context
{
    public class CMSDatabase : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }

        public DbSet<ConnectedUser> ConnectedUsers { get; set; }

        public DbSet<UsualPage> UsualPages { get; set; }
        public DbSet<CategoryPage> CategoryPages { get; set; }
        public DbSet<ProductPage> ProductPages { get; set; }
        
        public DbSet<Template> Templates { get; set; }
        public DbSet<Chunk> Chunks { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<Redirection> Redirections { get; set; }

        public DbSet<SynonymForString> SynonymsForStrings { get; set; }

        public CMSDatabase(DbContextOptions<CMSDatabase> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsualPage>().HasIndex(up => up.RequestPathHash);
            modelBuilder.Entity<CategoryPage>().HasIndex(cp => cp.RequestPathHash);
            modelBuilder.Entity<ProductPage>().HasIndex(pp => pp.RequestPathHash);
            modelBuilder.Entity<Image>().HasIndex(img => img.ShortPathHash);
            modelBuilder.Entity<Redirection>().HasIndex(r => r.RequestPathHash);
        }
    }
}