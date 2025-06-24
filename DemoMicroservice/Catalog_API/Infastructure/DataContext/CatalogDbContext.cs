using Catalog_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog_API.Infastructure.DataContext
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }
        public DbSet<Category> Categories { get; set; }
    }
} 