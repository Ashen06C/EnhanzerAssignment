using Enhanzer.Assignment.Application.Common;
using Enhanzer.Assignment.Application.Locations;
using Enhanzer.Assignment.Domain.Entities;
using Enhanzer.Assignment.Domain.Services;

namespace Enhanzer.Assignment.Application.PurchaseBills;

public sealed class PurchaseBillService(
    ILocationRepository locationRepository,
    IPurchaseBillRepository purchaseBillRepository)
{
    public async Task<SavePurchaseBillResponseDto> SaveAsync(
        string userEmail,
        SavePurchaseBillRequestDto request,
        CancellationToken cancellationToken)
    {
        var errors = await ValidateAsync(request, cancellationToken);
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var now = DateTime.UtcNow;
        var inputs = request.Items.Select(ToCalculationInput).ToList();
        var summary = PurchaseCalculationService.CalculateSummary(inputs);

        var bill = new PurchaseBill
        {
            Id = Guid.NewGuid(),
            UserEmail = userEmail,
            TotalItems = summary.TotalItems,
            TotalQuantity = summary.TotalQuantity,
            TotalCost = summary.TotalCost,
            TotalSelling = summary.TotalSelling,
            CreatedAtUtc = now
        };

        foreach (var item in request.Items)
        {
            var calculation = PurchaseCalculationService.CalculateLine(ToCalculationInput(item));
            bill.Items.Add(new PurchaseBillItem
            {
                ItemName = FruitCatalog.Normalize(item.ItemName),
                LocationCode = item.LocationCode.Trim(),
                LocationName = item.LocationName.Trim(),
                StandardCost = item.StandardCost,
                StandardPrice = item.StandardPrice,
                Quantity = item.Quantity,
                DiscountPercentage = item.DiscountPercentage,
                TotalCost = calculation.TotalCost,
                TotalSelling = calculation.TotalSelling
            });
        }

        await purchaseBillRepository.AddAsync(bill, cancellationToken);

        return new SavePurchaseBillResponseDto(
            bill.Id,
            bill.TotalItems,
            bill.TotalQuantity,
            bill.TotalCost,
            bill.TotalSelling);
    }

    private async Task<Dictionary<string, string[]>> ValidateAsync(
        SavePurchaseBillRequestDto request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (request.Items.Count == 0)
        {
            errors["items"] = ["At least one item is required."];
            return errors;
        }

        var index = 0;
        foreach (var item in request.Items)
        {
            var prefix = $"items[{index}]";

            if (!FruitCatalog.IsAllowed(item.ItemName))
            {
                errors[$"{prefix}.itemName"] = ["Item must be one of the allowed fruit values."];
            }

            if (string.IsNullOrWhiteSpace(item.LocationCode) ||
                !await locationRepository.ExistsAsync(item.LocationCode.Trim(), cancellationToken))
            {
                errors[$"{prefix}.locationCode"] = ["Batch must exist in Location_Details."];
            }

            if (item.StandardCost < 0m)
            {
                errors[$"{prefix}.standardCost"] = ["Standard Cost must be zero or greater."];
            }

            if (item.StandardPrice < 0m)
            {
                errors[$"{prefix}.standardPrice"] = ["Standard Price must be zero or greater."];
            }

            if (item.Quantity <= 0m)
            {
                errors[$"{prefix}.quantity"] = ["Quantity must be greater than zero."];
            }

            if (item.DiscountPercentage is < 0m or > 100m)
            {
                errors[$"{prefix}.discountPercentage"] = ["Discount must be between 0 and 100."];
            }

            index++;
        }

        return errors;
    }

    private static PurchaseLineInput ToCalculationInput(PurchaseBillItemDto item) =>
        new(
            item.ItemName,
            item.LocationCode,
            item.LocationName,
            item.StandardCost,
            item.StandardPrice,
            item.Quantity,
            item.DiscountPercentage);
}
