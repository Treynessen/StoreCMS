using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Treynessen.Database.Entities;

namespace Treynessen.Database.Context
{
    public static class PostgreSqlModel
    {
        public static void Configure(ModelBuilder model)
        {
            model.UseIdentityByDefaultColumns();
            ConfigurePage<UsualPage>(model, "UsualPages");
            ConfigurePage<CategoryPage>(model, "CategoryPages");
            ConfigurePage<ProductPage>(model, "ProductPages");
            model.Entity<AdminPanelLog>().HasBaseType((Type)null);

            model.Entity<User>().ToTable("Users");
            model.Entity<UserType>().ToTable("UserTypes");
            model.Entity<SecurityInstallation>().ToTable("SecurityInstallation");
            model.Entity<ConnectedUser>().ToTable("ConnectedUsers");
            model.Entity<Visitor>().ToTable("Visitors");
            model.Entity<VisitedPage>().ToTable("VisitedPages");
            model.Entity<Template>().ToTable("Templates");
            model.Entity<Chunk>().ToTable("Chunks");
            model.Entity<Image>().ToTable("Images");
            model.Entity<Redirection>().ToTable("Redirections");
            model.Entity<SynonymForString>().ToTable("SynonymsForStrings");
            model.Entity<AdminPanelLog>().ToTable("AdminPanelLogs");

            // [Комментарий ИИ-агента]: ID трёх видов страниц независимы; C#-наследование не создаёт общую EF-иерархию.
            foreach (var entity in model.Model.GetEntityTypes())
            {
                model.Entity(entity.ClrType).HasKey("ID");
                if (entity.ClrType != typeof(SecurityInstallation))
                    model.Entity(entity.ClrType).Property<int>("ID").UseIdentityByDefaultColumn();
            }
            model.Entity<SecurityInstallation>().Property(row => row.ID).ValueGeneratedNever();
            model.Entity<User>().Property(row => row.Password).IsConcurrencyToken();

            model.Entity<UsualPage>().HasOne(row => row.PreviousPage).WithMany()
                .HasForeignKey(row => row.PreviousPageID).OnDelete(DeleteBehavior.ClientSetNull);
            model.Entity<CategoryPage>().HasOne(row => row.PreviousPage).WithMany()
                .HasForeignKey(row => row.PreviousPageID).OnDelete(DeleteBehavior.ClientSetNull);
            model.Entity<CategoryPage>().HasOne(row => row.LastProductTemplate).WithMany()
                .HasForeignKey(row => row.LastProductTemplateID).OnDelete(DeleteBehavior.ClientSetNull);
            model.Entity<ProductPage>().HasOne(row => row.PreviousPage).WithMany(row => row.ProductPages)
                .HasForeignKey(row => row.PreviousPageID).IsRequired().OnDelete(DeleteBehavior.Cascade);
            model.Entity<User>().HasOne(row => row.UserType).WithMany(row => row.Users)
                .HasForeignKey(row => row.UserTypeID).IsRequired().OnDelete(DeleteBehavior.Restrict);
            model.Entity<AdminPanelLog>().HasOne(row => row.User).WithMany(row => row.AdminPanelLogs)
                .HasForeignKey(row => row.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            model.Entity<ConnectedUser>().HasOne(row => row.User).WithMany()
                .HasForeignKey(row => row.UserID).IsRequired().OnDelete(DeleteBehavior.Cascade);
            model.Entity<VisitedPage>().HasOne(row => row.Visitor).WithMany(row => row.VisitedPages)
                .HasForeignKey(row => row.VisitorId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            // [Комментарий ИИ-агента]: bigint и CHECK сохраняют все значения uint и прежние единицы цены.
            UInt(model.Entity<ProductPage>().Property(row => row.Price));
            UInt(model.Entity<ProductPage>().Property(row => row.OldPrice));
            UInt(model.Entity<CategoryPage>().Property(row => row.ProductsCount));
            UInt(model.Entity<Image>().Property(row => row.Width));
            UInt(model.Entity<Image>().Property(row => row.Height));
            UInt(model.Entity<User>().Property(row => row.IdleTime));
            model.Entity<ProductPage>().ToTable(table => {
                table.HasCheckConstraint("CK_ProductPages_Price_UInt32", "\"Price\" BETWEEN 0 AND 4294967295");
                table.HasCheckConstraint("CK_ProductPages_OldPrice_UInt32", "\"OldPrice\" BETWEEN 0 AND 4294967295");
            });
            model.Entity<CategoryPage>().ToTable(table => table.HasCheckConstraint("CK_CategoryPages_ProductsCount_UInt32", "\"ProductsCount\" BETWEEN 0 AND 4294967295"));
            model.Entity<Image>().ToTable(table => {
                table.HasCheckConstraint("CK_Images_Width_UInt32", "\"Width\" BETWEEN 0 AND 4294967295");
                table.HasCheckConstraint("CK_Images_Height_UInt32", "\"Height\" BETWEEN 0 AND 4294967295");
            });
            model.Entity<User>().ToTable(table => table.HasCheckConstraint("CK_Users_IdleTime", "\"IdleTime\" BETWEEN 10 AND 10080"));
            model.Entity<UserType>().ToTable(table => table.HasCheckConstraint("CK_UserTypes_AccessLevel", "\"AccessLevel\" BETWEEN 1 AND 5"));
            model.Entity<VisitedPage>().ToTable(table => table.HasCheckConstraint("CK_VisitedPages_PageType", "\"PageType\" BETWEEN 1 AND 3"));

            model.Entity<ConnectedUser>(entity => {
                entity.Property(row => row.TokenHash).HasColumnType("character varying(64)").HasMaxLength(64).UseCollation("C").IsRequired();
                entity.ToTable(table => table.HasCheckConstraint("CK_ConnectedUsers_TokenHash", "\"TokenHash\" ~ '^[0-9a-f]{64}$'"));
                entity.Property(row => row.LastActionUtc).HasColumnType("timestamp with time zone").HasPrecision(6);
                entity.Property(row => row.IPAdress).HasMaxLength(45);
                entity.Property(row => row.UserAgent).HasMaxLength(255);
                entity.HasIndex(row => row.TokenHash).IsUnique();
                entity.HasIndex(row => row.UserID).IsUnique();
            });
            model.Entity<SecurityInstallation>().Property(row => row.InitializedUtc).HasColumnType("timestamp with time zone").HasPrecision(6);
            // [Комментарий ИИ-агента]: Существующие локальные даты не переобозначаются как UTC.
            model.Entity<ProductPage>().Property(row => row.LastUpdate).HasColumnType("timestamp without time zone").HasPrecision(6);
            model.Entity<Visitor>().Property(row => row.FirstVisit).HasColumnType("timestamp without time zone").HasPrecision(6);
            model.Entity<Visitor>().Property(row => row.LastVisit).HasColumnType("timestamp without time zone").HasPrecision(6);
            model.Entity<AdminPanelLog>().Property(row => row.Time).HasColumnType("timestamp without time zone").HasPrecision(6);

            model.Entity<User>().Property(row => row.Login).UseCollation("C");
            model.Entity<User>().HasIndex(row => row.Login).IsUnique();
            model.Entity<Visitor>().Property(row => row.IPAdress).UseCollation("C");
            model.Entity<Visitor>().HasIndex(row => row.IPAdress).IsUnique();
            model.Entity<Visitor>().HasIndex(row => row.IPStringHash);
            model.Entity<Image>().Property(row => row.ShortPath).UseCollation("C");
            model.Entity<Image>().HasIndex(row => row.ShortPath).IsUnique();
            model.Entity<Image>().HasIndex(row => row.ShortPathHash);
            model.Entity<SynonymForString>().Property(row => row.String).UseCollation("C");
            model.Entity<SynonymForString>().Property(row => row.Synonym).UseCollation("C");
            model.Entity<SynonymForString>().HasIndex(row => new { row.String, row.Synonym }).IsUnique();
            model.Entity<Redirection>().Property(row => row.RequestPath).UseCollation("C");
            model.Entity<Redirection>().HasIndex(row => row.RequestPathHash);
        }

        private static void ConfigurePage<T>(ModelBuilder model, string table) where T : Page
        {
            var entity = model.Entity<T>();
            entity.HasBaseType((Type)null);
            entity.ToTable(table);
            entity.Property(row => row.RequestPath).UseCollation("C");
            entity.HasIndex(row => row.RequestPath).IsUnique();
            entity.HasIndex(row => row.RequestPathHash);
            entity.HasOne(row => row.Template).WithMany().HasForeignKey(row => row.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        private static void UInt(PropertyBuilder<uint> property) => property.HasConversion<long>().HasColumnType("bigint");
    }
}
