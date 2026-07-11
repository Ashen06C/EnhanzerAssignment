namespace Enhanzer.Assignment.Application.Auth;

public interface IAuthTicketService
{
    Task SignInAsync(string email, CancellationToken cancellationToken);
    Task SignOutAsync(CancellationToken cancellationToken);
}
