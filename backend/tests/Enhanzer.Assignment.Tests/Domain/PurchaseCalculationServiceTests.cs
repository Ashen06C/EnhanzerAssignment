using Enhanzer.Assignment.Domain.Services;
using Xunit;

namespace Enhanzer.Assignment.Tests.Domain;

public sealed class PurchaseCalculationServiceTests
{
    [Fact]
    public void CalculateLine_WithTwentyPercentDiscount_ReturnsPdfExampleTotals()
    {
        var result = PurchaseCalculationService.CalculateLine(CreateInput(100m, 150m, 5m, 20m));

        Assert.Equal(500m, result.GrossCost);
        Assert.Equal(100m, result.DiscountAmount);
        Assert.Equal(400m, result.TotalCost);
        Assert.Equal(750m, result.TotalSelling);
    }

    [Fact]
    public void CalculateLine_WithZeroDiscount_ReturnsGrossCost()
    {
        var result = PurchaseCalculationService.CalculateLine(CreateInput(20m, 30m, 2m, 0m));

        Assert.Equal(40m, result.TotalCost);
        Assert.Equal(60m, result.TotalSelling);
    }

    [Fact]
    public void CalculateLine_WithFullDiscount_ReturnsZeroTotalCost()
    {
        var result = PurchaseCalculationService.CalculateLine(CreateInput(20m, 30m, 2m, 100m));

        Assert.Equal(0m, result.TotalCost);
        Assert.Equal(60m, result.TotalSelling);
    }

    [Fact]
    public void CalculateLine_WithNegativeValues_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PurchaseCalculationService.CalculateLine(CreateInput(-1m, 30m, 2m, 0m)));
    }

    [Fact]
    public void CalculateSummary_SumsItems()
    {
        var summary = PurchaseCalculationService.CalculateSummary(
        [
            CreateInput(100m, 150m, 5m, 20m),
            CreateInput(10m, 15m, 2m, 0m)
        ]);

        Assert.Equal(2, summary.TotalItems);
        Assert.Equal(7m, summary.TotalQuantity);
        Assert.Equal(420m, summary.TotalCost);
        Assert.Equal(780m, summary.TotalSelling);
    }

    private static PurchaseLineInput CreateInput(
        decimal standardCost,
        decimal standardPrice,
        decimal quantity,
        decimal discountPercentage) =>
        new(
            "Mango",
            "LOC01",
            "Main Warehouse",
            standardCost,
            standardPrice,
            quantity,
            discountPercentage);
}
