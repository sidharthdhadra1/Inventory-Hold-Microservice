using InventoryHold.Domain.Entities;

namespace InventoryHold.Domain.Repositories;

public interface IInventoryRepository
{
    Task<ProductInventory> Get(string id);
    Task<bool> TryReserve(string id, int quantity);
    Task Release(string id, int quantity);
    Task<IEnumerable<ProductInventory>> ListAll();
}
