using invoice_backend.Models;
using invoice_backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace invoice_backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BaseEntity properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Configure primary key
                modelBuilder.Entity(entityType.ClrType)
                    .HasKey("Id");

                // Configure and index the Id column
                modelBuilder.Entity(entityType.ClrType)
                    .Property("Id")
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("Id")
                    .IsUnique();

                // Configure default values for audit properties
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity(entityType.ClrType)
                    .Property("ModifiedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAddOrUpdate();

                // Configure default values for status properties
                modelBuilder.Entity(entityType.ClrType)
                    .Property("Status")
                    .HasDefaultValue(Status.Active);

                modelBuilder.Entity(entityType.ClrType)
                    .Property("IsActive")
                    .HasDefaultValue(true);

                modelBuilder.Entity(entityType.ClrType)
                    .Property("IsArchived")
                    .HasDefaultValue(false);
            }
        }

        // Configure User entity
        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        modelBuilder.Entity<User>()
            .Property(u => u.CompanyName)
            .HasMaxLength(200);

        modelBuilder.Entity<User>()
            .Property(u => u.CompanyAddress)
            .HasMaxLength(300);

        modelBuilder.Entity<User>()
            .Property(u => u.CompanyCity)
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.CompanyState)
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.CompanyZipCode)
            .HasMaxLength(20);

        modelBuilder.Entity<User>()
            .Property(u => u.CompanyCountry)
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        // Configure Invoice entity
        modelBuilder.Entity<Invoice>()
            .Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .Property(i => i.InvoiceStatus)
            .HasDefaultValue(InvoiceStatus.Draft);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.BillerName)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.BillerAddress)
            .HasMaxLength(300);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.BillerEmail)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.ClientName)
            .IsRequired()
            .HasMaxLength(200);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.ClientAddress)
            .HasMaxLength(300);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.ClientEmail)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Currency)
            .IsRequired()
            .HasMaxLength(10);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.SubTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxPercentage)
            .HasPrecision(5, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Discount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Total)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Notes)
            .HasMaxLength(1000);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.LogoUrl)
            .HasMaxLength(500);

        // Configure relationship between Invoice and User
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure InvoiceItem entity
        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.Description)
            .IsRequired()
            .HasMaxLength(500);

        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.Quantity)
            .IsRequired();

        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.Rate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.Total)
            .HasPrecision(18, 2);

        // Configure relationship between Invoice and InvoiceItem
        modelBuilder.Entity<InvoiceItem>()
            .HasOne(ii => ii.Invoice)
            .WithMany(i => i.InvoiceItems)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
