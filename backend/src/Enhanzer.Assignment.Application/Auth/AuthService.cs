using Enhanzer.Assignment.Application.Common;
using Enhanzer.Assignment.Application.Locations;

namespace Enhanzer.Assignment.Application.Auth;

public sealed class AuthService(
    IExternalAuthClient externalAuthClient,
    ILocationRepository locationRepository,
    IAuthTicketService authTicketService)
{
    public async Task<AuthUserDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var errors = ValidateLoginRequest(request);
        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var email = request.Email.Trim();
        var result = await externalAuthClient.LoginAsync(new ExternalLoginRequest(email, request.Password), cancellationToken);

        await locationRepository.UpsertAsync(result.Locations, cancellationToken);
        await authTicketService.SignInAsync(email, cancellationToken);

        return new AuthUserDto(email, result.Locations);
    }

    public Task LogoutAsync(CancellationToken cancellationToken) =>
        authTicketService.SignOutAsync(cancellationToken);

    private static Dictionary<string, string[]> ValidateLoginRequest(LoginRequestDto request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }
        else if (!request.Email.Contains('@', StringComparison.Ordinal))
        {
            errors["email"] = ["Email must be valid."];
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = ["Password is required."];
        }

        return errors;
    }
}
