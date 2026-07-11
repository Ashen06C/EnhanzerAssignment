using Enhanzer.Assignment.Application.Locations;
using Xunit;

namespace Enhanzer.Assignment.Tests.Application;

public sealed class LocationUpsertTests
{
    [Fact]
    public async Task UpsertAsync_RepeatedLogins_DoNotDuplicateLocations()
    {
        var repository = new FakeLocationRepository();

        await repository.UpsertAsync([new LocationDto("LOC01", "Main Warehouse")], CancellationToken.None);
        await repository.UpsertAsync([new LocationDto("LOC01", "Updated Warehouse")], CancellationToken.None);

        var locations = await repository.GetAllAsync(CancellationToken.None);
        var location = Assert.Single(locations);
        Assert.Equal("LOC01", location.LocationCode);
        Assert.Equal("Updated Warehouse", location.LocationName);
    }
}
