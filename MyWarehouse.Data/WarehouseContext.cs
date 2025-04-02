﻿using Microsoft.EntityFrameworkCore;
using MyWarehouse.Data.Models;
using System.Data;

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
    public DbSet<Roles> Roles { get; set; }
    public DbSet<SupplierUsers> SupplierUsers { get; set; }

    public WarehouseContext(DbContextOptions<WarehouseContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        #region PRODUCTS
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
        #endregion

        #region ORDERS
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
        #endregion

        #region STATUSORDER
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
        #endregion

        #region ORDERDETAILS
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
        #endregion

        #region SUPPLIERS
        modelBuilder.Entity<Suppliers>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdSupplier");

            entity.HasOne(s => s.City)
                .WithMany(c => c.Suppliers)
                .HasForeignKey(s => s.IdCity);
        });
        #endregion

        #region USERS
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

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.IdRole);
        });
        #endregion

        #region SUPPLIERUSERS
        modelBuilder.Entity<SupplierUsers>(entity =>
        {
            entity.HasKey(su => new { su.IdSupplier, su.IdUser });

            entity.HasOne(su => su.Supplier)
                .WithMany(s => s.SupplierUsers)
                .HasForeignKey(su => su.IdSupplier);

            entity.HasOne(su => su.User)
                .WithMany(u => u.SupplierUsers)
                .HasForeignKey(su => su.IdUser);
        });
        #endregion

        #region CITIES
        modelBuilder.Entity<Cities>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdCity");
        });
        #endregion

        #region CATEGORIES
        modelBuilder.Entity<Categories>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdCategory");
        });
        #endregion

        #region ROLES
        modelBuilder.Entity<Roles>(entity =>
        {
            entity.Property(e => e.Id)
                .HasColumnName("IdRole");

            entity.HasData(
                new Roles { Id = 1, Name = "admin" },
                new Roles { Id = 2, Name = "client" },
                new Roles { Id = 3, Name = "usersupplier" }
            );
        });
        #endregion
    }

}

