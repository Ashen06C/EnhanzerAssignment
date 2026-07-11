namespace Enhanzer.Assignment.Domain.Entities;

public sealed class PurchaseBill
{
    public Guid Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<PurchaseBillItem> Items { get; set; } = [];
}
