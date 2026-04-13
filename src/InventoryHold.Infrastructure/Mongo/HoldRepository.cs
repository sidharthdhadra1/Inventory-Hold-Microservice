using InventoryHold.Domain.Entities;
using InventoryHold.Domain.Repositories;
using MongoDB.Driver;

namespace InventoryHold.Infrastructure.Mongo;

public class HoldRepository : IHoldRepository
{
    private readonly IMongoCollection<Hold> _coll;

    public HoldRepository(IMongoDatabase db)
    {
        _coll = db.GetCollection<Hold>("holds");
    }

    public async Task<Hold> Create(Hold hold)
    {
        await _coll.InsertOneAsync(hold);
        return hold;
    }

    public async Task<Hold> Get(string id)
    {
        return await _coll.Find(h => h.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Hold> Update(Hold hold)
    {
        await _coll.ReplaceOneAsync(h => h.Id == hold.Id, hold);
        return hold;
    }
}
