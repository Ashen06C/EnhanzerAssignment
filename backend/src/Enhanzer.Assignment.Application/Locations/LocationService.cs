namespace Enhanzer.Assignment.Application.Locations;

public sealed class LocationService(ILocationRepository locationRepository)
{
    public Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken) =>
        locationRepository.GetAllAsync(cancellationToken);
}
