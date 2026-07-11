using Enhanzer.Assignment.Application.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Assignment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/locations")]
public sealed class LocationsController(LocationService locationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LocationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var locations = await locationService.GetAllAsync(cancellationToken);
        return Ok(locations);
    }
}
