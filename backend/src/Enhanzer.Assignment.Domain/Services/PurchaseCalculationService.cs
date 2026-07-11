namespace Enhanzer.Assignment.Domain.Services;

public sealed record PurchaseLineInput(
    string ItemName,
    string LocationCode,
    string LocationName,
    decimal StandardCost,
    decimal StandardPrice,
    decimal Quantity,
    decimal DiscountPercentage);

public sealed record PurchaseLineCalculation(
    decimal GrossCost,
    decimal DiscountAmount,
    decimal TotalCost,
    decimal TotalSelling);

public sealed record PurchaseBillSummary(
    int TotalItems,
    decimal TotalQuantity,
    decimal TotalCost,
    decimal TotalSelling);

public static class PurchaseCalculationService
{
    public static PurchaseLineCalculation CalculateLine(PurchaseLineInput input)
    {
        ValidateInput(input);

        var grossCost = RoundMoney(input.StandardCost * input.Quantity);
        var discountAmount = RoundMoney(grossCost * input.DiscountPercentage / 100m);
        var totalCost = RoundMoney(grossCost - discountAmount);
        var totalSelling = RoundMoney(input.StandardPrice * input.Quantity);

        return new PurchaseLineCalculation(grossCost, discountAmount, totalCost, totalSelling);
    }

    public static PurchaseBillSummary CalculateSummary(IEnumerable<PurchaseLineInput> inputs)
    {
        var items = inputs.ToList();
        var calculations = items.Select(CalculateLine).ToList();

        return new PurchaseBillSummary(
            items.Count,
            RoundMoney(items.Sum(item => item.Quantity)),
            RoundMoney(calculations.Sum(item => item.TotalCost)),
            RoundMoney(calculations.Sum(item => item.TotalSelling)));
    }

    public static decimal RoundMoney(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static void ValidateInput(PurchaseLineInput input)
    {
        if (!FruitCatalog.IsAllowed(input.ItemName))
        {
            throw new ArgumentException("Item is not allowed.", nameof(input));
        }

        if (input.StandardCost < 0m || input.StandardPrice < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Costs and prices must be zero or greater.");
        }

        if (input.Quantity <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Quantity must be greater than zero.");
        }

        if (input.DiscountPercentage is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Discount must be between 0 and 100.");
        }
    }
}
