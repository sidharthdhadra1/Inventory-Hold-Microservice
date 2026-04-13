using System.Text.Json;
using InventoryHold.Domain.Entities;
using InventoryHold.Domain.Repositories;
using InventoryHold.Infrastructure.Redis;
using MongoDB.Driver;

namespace InventoryHold.Infrastructure.Mongo;

public class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<ProductInventory> _coll;
    private readonly ICacheService _cache;
    private const string CacheKey = "inventory:all";

    public InventoryRepository(IMongoDatabase db, ICacheService cache)
    {
        _coll = db.GetCollection<ProductInventory>("inventory");
        _cache = cache;
    }

    public async Task<ProductInventory> Get(string id)
    {
        return await _coll.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ProductInventory>> ListAll()
    {
        // try cache
        var cached = await _cache.GetStringAsync(CacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<IEnumerable<ProductInventory>>(cached);
        }

        var list = await _coll.Find(_ => true).ToListAsync();
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(list), TimeSpan.FromSeconds(10));
        return list;
    }

    public async Task<bool> TryReserve(string id, int quantity)
    {
        var filter = Builders<ProductInventory>.Filter.And(
            Builders<ProductInventory>.Filter.Eq(p => p.Id, id),
            Builders<ProductInventory>.Filter.Gte(p => p.Quantity, quantity)
        );

        var update = Builders<ProductInventory>.Update.Inc(p => p.Quantity, -quantity);

        var res = await _coll.UpdateOneAsync(filter, update);
        if (res.ModifiedCount == 1)
        {
            await _cache.RemoveAsync(CacheKey);
            return true;
        }

        return false;
    }

    public async Task Release(string id, int quantity)
    {
        var update = Builders<ProductInventory>.Update.Inc(p => p.Quantity, quantity);
        await _coll.UpdateOneAsync(p => p.Id == id, update);
        await _cache.RemoveAsync(CacheKey);
    }
}
