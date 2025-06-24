using Microsoft.EntityFrameworkCore;
using Product_API.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Product_API.Infastructure.DataContext;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}
