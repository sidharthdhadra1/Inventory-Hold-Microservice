using InventoryHold.Domain.Entities;
using MongoDB.Driver;

namespace InventoryHold.Infrastructure.Mongo;

public static class SeedData
{
    public static async Task EnsureSeed(IMongoDatabase db)
    {
        var coll = db.GetCollection<ProductInventory>("inventory");
        var count = await coll.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            await coll.InsertManyAsync(new[] {
                new ProductInventory { Name = "Widget A", Quantity = 100 },
                new ProductInventory { Name = "Widget B", Quantity = 50 },
                new ProductInventory { Name = "Widget C", Quantity = 75 },
                new ProductInventory { Name = "Widget D", Quantity = 20 },
                new ProductInventory { Name = "Widget E", Quantity = 5 }
            });
        }
    }
}
