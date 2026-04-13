using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace InventoryHold.Domain.Entities;

public class ProductInventory
{
    // Keep Mongo attributes for Mongo implementation but add DataAnnotations for EF
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
}
