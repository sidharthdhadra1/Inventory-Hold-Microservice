using InventoryHold.Domain.Entities;

namespace InventoryHold.Domain.Repositories;

public interface IHoldRepository
{
    Task<Hold> Create(Hold hold);
    Task<Hold> Get(string id);
    Task<Hold> Update(Hold hold);
}
