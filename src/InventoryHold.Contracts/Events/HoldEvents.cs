namespace InventoryHold.Contracts.Events;

public record HoldCreated(string HoldId, string ProductId, int Quantity, DateTime ExpiresAt);
public record HoldReleased(string HoldId, string ProductId, int Quantity);
public record HoldExpired(string HoldId, string ProductId, int Quantity);
