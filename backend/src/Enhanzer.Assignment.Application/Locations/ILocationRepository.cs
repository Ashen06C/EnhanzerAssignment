namespace Enhanzer.Assignment.Application.Locations;

public interface ILocationRepository
{
    Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string locationCode, CancellationToken cancellationToken);
    Task UpsertAsync(IEnumerable<LocationDto> locations, CancellationToken cancellationToken);
}
