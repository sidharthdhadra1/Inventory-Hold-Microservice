namespace InventoryHold.Infrastructure.Redis;

public interface ICacheService
{
    Task<string> GetStringAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? ttl = null);
    Task RemoveAsync(string key);
}
