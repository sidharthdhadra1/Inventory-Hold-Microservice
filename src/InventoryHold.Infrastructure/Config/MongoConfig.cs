using MongoDB.Driver;

namespace InventoryHold.Infrastructure.Config;

public static class MongoConfig
{
    public static IMongoDatabase Create(string conn, string dbName = "inventory")
    {
        var client = new MongoClient(conn);
        return client.GetDatabase(dbName);
    }
}
