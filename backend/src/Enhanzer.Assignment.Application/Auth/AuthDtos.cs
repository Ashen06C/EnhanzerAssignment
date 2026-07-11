using Enhanzer.Assignment.Application.Locations;

namespace Enhanzer.Assignment.Application.Auth;

public sealed record LoginRequestDto(string Email, string Password);

public sealed record AuthUserDto(string Email, IReadOnlyCollection<LocationDto> Locations);

public sealed record ExternalLoginRequest(string Email, string Password);

public sealed record ExternalLoginResult(string Email, IReadOnlyCollection<LocationDto> Locations);
