using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data.Models;

namespace MyWarehouse.Data;


public class WarehouseContext : DbContext
{
    public DbSet<Categories> Categories { get; set; }
    public DbSet<Users> Users { get; set; }
    public DbSet<Cities> Cities { get; set; }
    public DbSet<Suppliers> Suppliers { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrderDetails> OrderDetails { get; set; }
    public DbSet<StatusOrders> StatusOrders { get; set; }

    public WarehouseContext(DbContextOptions<WarehouseContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Products>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdProduct");

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.IdCategory);

            entity.HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.IdSupplier);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Orders>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdOrder");

            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.IdUser);

            entity.HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.IdStatus);

            entity.Property(p => p.OrderDate)
                .HasDefaultValueSql("GETDATE()");
            
            entity.Property(o => o.TotalPrice)
                .HasColumnType("decimal(10,2)");

        });

        modelBuilder.Entity<StatusOrders>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdStatus");

            entity.HasMany(o => o.Orders)
                .WithOne(s => s.Status)
                .HasForeignKey(o => o.IdStatus);

            entity.HasData(
                new StatusOrders { Id = 1, Description = "In Lavorazione"},
                new StatusOrders { Id = 2, Description = "Spedito" },
                new StatusOrders { Id = 3, Description = "Consegnato" },
                new StatusOrders { Id = 4, Description = "Annullato" }
                );
        });

        modelBuilder.Entity<OrderDetails>(entity =>
        {
            entity.HasKey(od => new { od.IdOrder, od.IdProduct });

            entity.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.IdOrder);

            entity.HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.IdProduct);

            entity.Property(od => od.UnitPrice)
                .HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Suppliers>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdSupplier");

            entity.HasOne(s => s.City)
                .WithMany(c => c.Suppliers)
                .HasForeignKey(s => s.IdCity);
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdUser");

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<Cities>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdCity");
        });

        modelBuilder.Entity<Categories>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdCategory");
        });
    }

}

