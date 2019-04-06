using Microsoft.EntityFrameworkCore;
using Trane.Database.Entities;
using Trane.OtherTypes;

namespace Trane.Database.Context
{
    public class CMSDatabase : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }

        public DbSet<ConnectedUser> ConnectedUsers { get; set; }

        public DbSet<SimplePage> SimplePages { get; set; }
        public DbSet<CategoryPage> CategoryPages { get; set; }
        public DbSet<ProductPage> ProductPages { get; set; }

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
                AccessLevel = AccessLevels.VeryHigh
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
