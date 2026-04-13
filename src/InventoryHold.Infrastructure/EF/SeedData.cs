using InventoryHold.Domain.Entities;

namespace InventoryHold.Infrastructure.EF;

public static class SeedData
{
    public static async Task EnsureSeed(InventoryDbContext db)
    {
        // Ensure database and tables exist
        await db.Database.EnsureCreatedAsync();

        if (!db.ProductInventories.Any())
        {
            db.ProductInventories.AddRange(new[] {
                new ProductInventory { Id = Guid.NewGuid().ToString(), Name = "Widget A", Quantity = 100 },
                new ProductInventory { Id = Guid.NewGuid().ToString(), Name = "Widget B", Quantity = 50 },
                new ProductInventory { Id = Guid.NewGuid().ToString(), Name = "Widget C", Quantity = 75 },
                new ProductInventory { Id = Guid.NewGuid().ToString(), Name = "Widget D", Quantity = 20 },
                new ProductInventory { Id = Guid.NewGuid().ToString(), Name = "Widget E", Quantity = 5 }
            });
            await db.SaveChangesAsync();
        }
    }
}
