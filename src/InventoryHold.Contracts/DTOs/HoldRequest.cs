namespace InventoryHold.Contracts.DTOs;

public class HoldRequest
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public string? CustomerId { get; set; }
}
