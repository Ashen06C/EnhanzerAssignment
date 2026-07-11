using Enhanzer.Assignment.Application.Common;
using Enhanzer.Assignment.Application.Locations;
using Enhanzer.Assignment.Application.PurchaseBills;
using Enhanzer.Assignment.Domain.Entities;
using Xunit;

namespace Enhanzer.Assignment.Tests.Application;

public sealed class PurchaseBillServiceTests
{
    [Fact]
    public async Task SaveAsync_WithValidItem_PersistsCalculatedBill()
    {
        var locations = new FakeLocationRepository();
        await locations.UpsertAsync([new LocationDto("LOC01", "Main Warehouse")], CancellationToken.None);
        var bills = new FakePurchaseBillRepository();
        var service = new PurchaseBillService(locations, bills);

        var response = await service.SaveAsync(
            "user@example.com",
            new SavePurchaseBillRequestDto(
            [
                new PurchaseBillItemDto("Mango", "LOC01", "Main Warehouse", 100m, 150m, 5m, 20m)
            ]),
            CancellationToken.None);

        Assert.Equal(1, response.TotalItems);
        Assert.Equal(5m, response.TotalQuantity);
        Assert.Equal(400m, response.TotalCost);
        Assert.Equal(750m, response.TotalSelling);
        Assert.Single(bills.Bills);
    }

    [Fact]
    public async Task SaveAsync_WithMissingLocation_ReturnsValidationErrors()
    {
        var service = new PurchaseBillService(new FakeLocationRepository(), new FakePurchaseBillRepository());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            service.SaveAsync(
                "user@example.com",
                new SavePurchaseBillRequestDto(
                [
                    new PurchaseBillItemDto("Mango", "UNKNOWN", "Unknown", 100m, 150m, 5m, 20m)
                ]),
                CancellationToken.None));

        Assert.Contains("items[0].locationCode", ex.Errors.Keys);
    }
}

public sealed class FakeLocationRepository : ILocationRepository
{
    private readonly Dictionary<string, LocationDto> locations = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<LocationDto>>(locations.Values.ToList());

    public Task<bool> ExistsAsync(string locationCode, CancellationToken cancellationToken) =>
        Task.FromResult(locations.ContainsKey(locationCode));

    public Task UpsertAsync(IEnumerable<LocationDto> locationsToUpsert, CancellationToken cancellationToken)
    {
        foreach (var location in locationsToUpsert)
        {
            locations[location.LocationCode] = location;
        }

        return Task.CompletedTask;
    }
}

public sealed class FakePurchaseBillRepository : IPurchaseBillRepository
{
    public List<PurchaseBill> Bills { get; } = [];

    public Task AddAsync(PurchaseBill bill, CancellationToken cancellationToken)
    {
        Bills.Add(bill);
        return Task.CompletedTask;
    }
}
