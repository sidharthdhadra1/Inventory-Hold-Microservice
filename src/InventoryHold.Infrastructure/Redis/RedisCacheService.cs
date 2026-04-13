using StackExchange.Redis;

namespace InventoryHold.Infrastructure.Redis;

public class RedisCacheService : ICacheService, IDisposable
{
    private readonly ConnectionMultiplexer _conn;
    private readonly IDatabase _db;

    public RedisCacheService(string conn)
    {
        _conn = ConnectionMultiplexer.Connect(conn);
        _db = _conn.GetDatabase();
    }

    public Task<string> GetStringAsync(string key)
    {
        var val = _db.StringGet(key);
        return Task.FromResult(val.HasValue ? (string)val : null);
    }

    public Task SetStringAsync(string key, string value, TimeSpan? ttl = null)
    {
        _db.StringSet(key, value, ttl);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _db.KeyDelete(key);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _conn?.Dispose();
    }
}
