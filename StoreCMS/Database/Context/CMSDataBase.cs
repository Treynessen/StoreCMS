using Microsoft.EntityFrameworkCore;
using Treynessen.Database.Entities;
using Treynessen.Security;

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

        public CMSDatabase(DbContextOptions<CMSDatabase> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserType>().HasData(new UserType
            {
                ID = 1,
                Name = "Admin",
                AccessLevel = AccessLevel.VeryHigh
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
