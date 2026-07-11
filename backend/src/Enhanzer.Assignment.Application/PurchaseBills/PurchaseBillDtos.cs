namespace Enhanzer.Assignment.Application.PurchaseBills;

public sealed record SavePurchaseBillRequestDto(IReadOnlyCollection<PurchaseBillItemDto> Items);

public sealed record PurchaseBillItemDto(
    string ItemName,
    string LocationCode,
    string LocationName,
    decimal StandardCost,
    decimal StandardPrice,
    decimal Quantity,
    decimal DiscountPercentage);

public sealed record SavePurchaseBillResponseDto(
    Guid BillId,
    int TotalItems,
    decimal TotalQuantity,
    decimal TotalCost,
    decimal TotalSelling);
