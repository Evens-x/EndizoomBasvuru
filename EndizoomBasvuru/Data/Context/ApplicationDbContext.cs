using EndizoomBasvuru.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EndizoomBasvuru.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<CompanyImage> CompanyImages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure cascade delete for Document-Company relation
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascade delete for CompanyImage-Company relation
            modelBuilder.Entity<CompanyImage>()
                .HasOne(i => i.Company)
                .WithMany(c => c.Images)
                .HasForeignKey(i => i.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed admin users with fixed creation date
            var fixedDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Sistem ilk çalıştırıldığında veritabanında oluşacak Base Admin kullanıcısı
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    Username = "superadmin",
                    Password = BCrypt.Net.BCrypt.HashPassword("EndizoomAdmin2023!"), // Güçlü şifre
                    Email = "admin@endizoom.com.tr",
                    FirstName = "Endizoom",
                    LastName = "Administrator",
                    Role = UserRole.Admin,
                    CreatedAt = fixedDate
                },
    new Admin
    {
        Id = 2,
        Username = "marketing",
        Password = BCrypt.Net.BCrypt.HashPassword("EndizoomMarketing2023!"), // Güçlü şifre
        Email = "marketing@endizoom.com.tr",
        FirstName = "Endizoom",
        LastName = "Marketing",
        Role = UserRole.Marketing,
        CreatedAt = fixedDate
    }
            );
        }
    }
}