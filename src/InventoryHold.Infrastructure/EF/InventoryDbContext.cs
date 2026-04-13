using InventoryHold.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryHold.Infrastructure.EF;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<ProductInventory> ProductInventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ProductInventory>().HasKey(p => p.Id);
    }
}
