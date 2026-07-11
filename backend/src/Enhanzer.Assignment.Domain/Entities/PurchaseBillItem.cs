namespace Enhanzer.Assignment.Domain.Entities;

public sealed class PurchaseBillItem
{
    public long Id { get; set; }
    public Guid PurchaseBillId { get; set; }
    public PurchaseBill? PurchaseBill { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public decimal StandardCost { get; set; }
    public decimal StandardPrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalSelling { get; set; }
}
