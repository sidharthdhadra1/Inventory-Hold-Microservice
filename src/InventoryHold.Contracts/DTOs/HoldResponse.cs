namespace InventoryHold.Contracts.DTOs;

public class HoldResponse
{
    public string HoldId { get; set; }
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
}
