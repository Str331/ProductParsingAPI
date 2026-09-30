using Microsoft.EntityFrameworkCore;
using ProductParsing.Extensions.Common;

namespace ProductParsing.Extensions.Data
{
    public class ProductParsingContext(DbContextOptions<ProductParsingContext> options) : DbContext(options)
    {
        public const string _sourceUrlIndexName = "IX_Products_SourceUrl";

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.HasIndex(e => e.SourceUrl, _sourceUrlIndexName).IsUnique().HasFilter("[SourceUrl] IS NOT NULL");
                entity.HasIndex(e => e.CreatedAtUtc, "IX_Products_CreatedAtUtc").IsDescending();

                entity.Property(e => e.Name).HasMaxLength(Product.NameMaxLength);
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.SourceUrl).HasMaxLength(Product.SourceUrlMaxLength);
                entity.Property(e => e.SourceHost).HasMaxLength(100);

                entity.HasOne(e => e.Image)
                      .WithOne()
                      .HasForeignKey<ProductImage>(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(e => e.ProductId);

                entity.Property(e => e.ProductId).ValueGeneratedNever();
                entity.Property(e => e.Content).HasColumnType("varbinary(max)");
                entity.Property(e => e.ContentType).HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.SourceUrl).HasMaxLength(ProductImage.SourceUrlMaxLength);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(Role.NameMaxLength);

                entity.HasData(new Role { Id = (int)UserRole.Admin, Name = nameof(UserRole.Admin) },
                               new Role { Id = (int)UserRole.User, Name = nameof(UserRole.User) });
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(e => e.UserName).IsUnique();

                entity.Property(e => e.UserName).HasMaxLength(User.UserNameMaxLength);
                entity.Property(e => e.PasswordHash).HasMaxLength(200);

                entity.HasOne(e => e.Role)
                      .WithMany()
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
