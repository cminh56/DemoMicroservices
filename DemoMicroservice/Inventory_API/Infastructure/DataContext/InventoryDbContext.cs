using Microsoft.EntityFrameworkCore;
using Inventory_API.Domain.Entities;

namespace Inventory_API.Infastructure.DataContext;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Inventory> Inventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.ReservedQuantity).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Create unique index on ProductId
            entity.HasIndex(e => e.ProductId).IsUnique();
        });
    }
} 