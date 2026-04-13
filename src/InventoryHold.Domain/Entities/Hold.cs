using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace InventoryHold.Domain.Entities;

public class Hold
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public string CustomerId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool Released { get; set; }
}
