using InventoryHold.Contracts.DTOs;
using InventoryHold.Contracts.Events;
using InventoryHold.Domain.Entities;
using InventoryHold.Domain.Repositories;

namespace InventoryHold.Domain.Services;

public class HoldService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IHoldRepository _holdRepository;
    private readonly IMessagePublisher _publisher;
    private readonly TimeSpan _holdDuration;

    public HoldService(IInventoryRepository inventoryRepository, IHoldRepository holdRepository, IMessagePublisher publisher, TimeSpan? holdDuration = null)
    {
        _inventoryRepository = inventoryRepository;
        _holdRepository = holdRepository;
        _publisher = publisher;
        _holdDuration = holdDuration ?? TimeSpan.FromMinutes(15);
    }

    public async Task<HoldResponse> CreateHold(HoldRequest req)
    {
        var ok = await _inventoryRepository.TryReserve(req.ProductId, req.Quantity);
        if (!ok) throw new InvalidOperationException("Insufficient stock");

        var hold = new Hold
        {
            ProductId = req.ProductId,
            Quantity = req.Quantity,
            CustomerId = req.CustomerId,
            ExpiresAt = DateTime.UtcNow.Add(_holdDuration),
            Released = false
        };

        var created = await _holdRepository.Create(hold);
        await _publisher.Publish(new HoldCreated(created.Id, created.ProductId, created.Quantity, created.ExpiresAt));

        return new HoldResponse
        {
            HoldId = created.Id,
            ProductId = created.ProductId,
            Quantity = created.Quantity,
            ExpiresAt = created.ExpiresAt
        };
    }

    public async Task<HoldResponse> GetHold(string id)
    {
        var hold = await _holdRepository.Get(id);
        if (hold == null) return null;
        if (hold.ExpiresAt <= DateTime.UtcNow && !hold.Released)
        {
            // publish expired
            await _publisher.Publish(new HoldExpired(hold.Id, hold.ProductId, hold.Quantity));
            // release inventory
            await _inventoryRepository.Release(hold.ProductId, hold.Quantity);
            hold.Released = true;
            await _holdRepository.Update(hold);
        }

        return new HoldResponse
        {
            HoldId = hold.Id,
            ProductId = hold.ProductId,
            Quantity = hold.Quantity,
            ExpiresAt = hold.ExpiresAt
        };
    }

    public async Task ReleaseHold(string id)
    {
        var hold = await _holdRepository.Get(id);
        if (hold == null) throw new KeyNotFoundException("Hold not found");
        if (hold.Released) return;

        await _inventoryRepository.Release(hold.ProductId, hold.Quantity);
        hold.Released = true;
        await _holdRepository.Update(hold);
        await _publisher.Publish(new HoldReleased(hold.Id, hold.ProductId, hold.Quantity));
    }
}

// IMessagePublisher is defined in a separate file; no duplicate here.
