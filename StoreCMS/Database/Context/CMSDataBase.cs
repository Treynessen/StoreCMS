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
            modelBuilder.Entity<SecurityInstallation>().Property(s => s.ID).ValueGeneratedNever();
            modelBuilder.Entity<User>().Property(u => u.Password).IsConcurrencyToken();
            modelBuilder.Entity<UsualPage>(entity =>
            {
                entity.HasIndex(up => up.RequestPath).IsUnique();
                entity.HasIndex(up => up.RequestPathHash);
            });
            modelBuilder.Entity<CategoryPage>(entity =>
            {
                entity.HasIndex(cp => cp.RequestPath).IsUnique();
                entity.HasIndex(cp => cp.RequestPathHash);
            });
            modelBuilder.Entity<ProductPage>(entity =>
            {
                entity.HasIndex(pp => pp.RequestPath).IsUnique();
                entity.HasIndex(pp => pp.RequestPathHash);
            });
            modelBuilder.Entity<Visitor>(entity =>
            {
                entity.HasIndex(v => v.IPAdress).IsUnique();
                entity.HasIndex(v => v.IPStringHash);
            });
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasIndex(img => img.ShortPath).IsUnique();
                entity.HasIndex(img => img.ShortPathHash);
            });
            modelBuilder.Entity<ConnectedUser>(entity =>
            {
                entity.Property(cu => cu.TokenHash).HasColumnType("char(64) CHARACTER SET ascii COLLATE ascii_bin").IsRequired();
                entity.Property(cu => cu.LastActionUtc).HasColumnType("datetime(6)");
                entity.Property(cu => cu.IPAdress).HasMaxLength(45);
                entity.Property(cu => cu.UserAgent).HasMaxLength(255);
                entity.HasIndex(cu => cu.TokenHash).IsUnique();
                entity.HasIndex(cu => cu.UserID).IsUnique();
            });
            modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
            modelBuilder.Entity<SynonymForString>().HasIndex(sfs => new { sfs.String, sfs.Synonym }).IsUnique();
            modelBuilder.Entity<Redirection>().HasIndex(r => r.RequestPathHash);
        }
    }
}
