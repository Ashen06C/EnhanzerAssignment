using System.Security.Claims;
using Enhanzer.Assignment.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Enhanzer.Assignment.Api.Services;

public sealed class CookieAuthTicketService(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration) : IAuthTicketService
{
    public async Task SignInAsync(string email, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("HTTP context is not available.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var expiresUtc = DateTimeOffset.UtcNow.AddMinutes(
            configuration.GetValue("Authentication:ExpirationMinutes", 60));

        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = expiresUtc
            });
    }

    public async Task SignOutAsync(CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("HTTP context is not available.");

        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
