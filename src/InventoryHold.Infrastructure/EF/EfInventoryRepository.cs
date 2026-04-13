using InventoryHold.Domain.Entities;
using InventoryHold.Domain.Repositories;
using InventoryHold.Infrastructure.Redis;
using Microsoft.EntityFrameworkCore;

namespace InventoryHold.Infrastructure.EF;

public class EfInventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _db;
    private readonly ICacheService _cache;
    private const string CacheKey = "inventory:all";

    public EfInventoryRepository(InventoryDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<ProductInventory> Get(string id)
    {
        return await _db.ProductInventories.FindAsync(id);
    }

    public async Task<IEnumerable<ProductInventory>> ListAll()
    {
        var cached = await _cache.GetStringAsync(CacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<ProductInventory>>(cached);
        }

        var list = await _db.ProductInventories.ToListAsync();
        await _cache.SetStringAsync(CacheKey, System.Text.Json.JsonSerializer.Serialize(list), TimeSpan.FromSeconds(10));
        return list;
    }

    public async Task<bool> TryReserve(string id, int quantity)
    {
        // Use a transaction to ensure concurrency safety in EF+SQLite for demo purposes
        using var tx = await _db.Database.BeginTransactionAsync();
        var item = await _db.ProductInventories.FirstOrDefaultAsync(p => p.Id == id);
        if (item == null || item.Quantity < quantity) return false;
        item.Quantity -= quantity;
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        await _cache.RemoveAsync(CacheKey);
        return true;
    }

    public async Task Release(string id, int quantity)
    {
        var item = await _db.ProductInventories.FirstOrDefaultAsync(p => p.Id == id);
        if (item == null) return;
        item.Quantity += quantity;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(CacheKey);
    }
}
