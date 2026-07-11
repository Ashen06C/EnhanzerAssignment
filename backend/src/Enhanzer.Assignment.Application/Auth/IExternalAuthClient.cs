namespace Enhanzer.Assignment.Application.Auth;

public interface IExternalAuthClient
{
    Task<ExternalLoginResult> LoginAsync(ExternalLoginRequest request, CancellationToken cancellationToken);
}
