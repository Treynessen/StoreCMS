using Trane.Db.Entities;
using Trane.Db.Entities.TypesForEntities;
using Microsoft.EntityFrameworkCore;

namespace Trane.Db.Context
{
    public class CMSContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }

        public DbSet<ConnectedUser> ConnectedUsers { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }

        public DbSet<SimplePage> SimplePages { get; set; }
        public DbSet<CategoryPage> CategoryPages { get; set; }
        public DbSet<ProductPage> ProductPages { get; set; }

        public CMSContext(DbContextOptions<CMSContext> options)
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
                ClearanceLevel = ClearanceLevel.VeryHigh
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}