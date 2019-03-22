using Trane.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace Trane.Db.Context
{
    public class CMSContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }

        public CMSContext(DbContextOptions<CMSContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserType>().HasData(new UserType { ID = 1, Name = "Admin" });
            base.OnModelCreating(modelBuilder);
        }
    }
}