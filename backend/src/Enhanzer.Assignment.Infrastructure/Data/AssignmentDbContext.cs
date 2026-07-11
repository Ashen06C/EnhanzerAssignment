using Enhanzer.Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Assignment.Infrastructure.Data;

public sealed class AssignmentDbContext(DbContextOptions<AssignmentDbContext> options) : DbContext(options)
{
    public DbSet<LocationDetail> LocationDetails => Set<LocationDetail>();
    public DbSet<PurchaseBill> PurchaseBills => Set<PurchaseBill>();
    public DbSet<PurchaseBillItem> PurchaseBillItems => Set<PurchaseBillItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("Location_Details");
            entity.HasKey(location => location.Id);
            entity.Property(location => location.Id).HasColumnName("Id");
            entity.Property(location => location.LocationCode).HasColumnName("Location_Code").HasMaxLength(50).IsRequired();
            entity.Property(location => location.LocationName).HasColumnName("Location_Name").HasMaxLength(200).IsRequired();
            entity.Property(location => location.CreatedAtUtc).HasColumnName("Created_At_Utc").IsRequired();
            entity.Property(location => location.UpdatedAtUtc).HasColumnName("Updated_At_Utc").IsRequired();
            entity.HasIndex(location => location.LocationCode).IsUnique().HasDatabaseName("UX_Location_Details_Location_Code");
        });

        modelBuilder.Entity<PurchaseBill>(entity =>
        {
            entity.ToTable("Purchase_Bills");
            entity.HasKey(bill => bill.Id);
            entity.Property(bill => bill.Id).HasColumnName("Id");
            entity.Property(bill => bill.UserEmail).HasColumnName("User_Email").HasMaxLength(256).IsRequired();
            entity.Property(bill => bill.TotalItems).HasColumnName("Total_Items").IsRequired();
            entity.Property(bill => bill.TotalQuantity).HasColumnName("Total_Quantity").HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.TotalCost).HasColumnName("Total_Cost").HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.TotalSelling).HasColumnName("Total_Selling").HasColumnType("decimal(18,2)");
            entity.Property(bill => bill.CreatedAtUtc).HasColumnName("Created_At_Utc").IsRequired();
            entity.HasMany(bill => bill.Items).WithOne(item => item.PurchaseBill).HasForeignKey(item => item.PurchaseBillId);
        });

        modelBuilder.Entity<PurchaseBillItem>(entity =>
        {
            entity.ToTable("Purchase_Bill_Items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasColumnName("Id");
            entity.Property(item => item.PurchaseBillId).HasColumnName("Purchase_Bill_Id");
            entity.Property(item => item.ItemName).HasColumnName("Item_Name").HasMaxLength(100).IsRequired();
            entity.Property(item => item.LocationCode).HasColumnName("Location_Code").HasMaxLength(50).IsRequired();
            entity.Property(item => item.LocationName).HasColumnName("Location_Name").HasMaxLength(200).IsRequired();
            entity.Property(item => item.StandardCost).HasColumnName("Standard_Cost").HasColumnType("decimal(18,2)");
            entity.Property(item => item.StandardPrice).HasColumnName("Standard_Price").HasColumnType("decimal(18,2)");
            entity.Property(item => item.Quantity).HasColumnName("Quantity").HasColumnType("decimal(18,2)");
            entity.Property(item => item.DiscountPercentage).HasColumnName("Discount_Percentage").HasColumnType("decimal(5,2)");
            entity.Property(item => item.TotalCost).HasColumnName("Total_Cost").HasColumnType("decimal(18,2)");
            entity.Property(item => item.TotalSelling).HasColumnName("Total_Selling").HasColumnType("decimal(18,2)");
        });
    }
}
