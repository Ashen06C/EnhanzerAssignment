using Enhanzer.Assignment.Api.Middleware;
using Enhanzer.Assignment.Api.Services;
using Enhanzer.Assignment.Application.Auth;
using Enhanzer.Assignment.Application.Locations;
using Enhanzer.Assignment.Application.PurchaseBills;
using Enhanzer.Assignment.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// Controllers, Swagger and health checks
// ---------------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------
// Application services
// ---------------------------------------------------------

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<PurchaseBillService>();
builder.Services.AddScoped<IAuthTicketService, CookieAuthTicketService>();

builder.Services.AddInfrastructure(builder.Configuration);

// ---------------------------------------------------------
// Railway reverse proxy support
// ---------------------------------------------------------

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Railway proxy addresses are dynamic.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ---------------------------------------------------------
// Cookie authentication
// ---------------------------------------------------------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-EnhanzerAssignment";
        options.Cookie.HttpOnly = true;

        // Required because Vercel and Railway use different domains.
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // Required for cookies whose names start with "__Host-".
        options.Cookie.Path = "/";

        options.SlidingExpiration = true;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(
            builder.Configuration.GetValue<int>(
                "Authentication:ExpirationMinutes",
                60));

        // Return API status codes instead of HTML redirects.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------
// Model validation response
// ---------------------------------------------------------

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(
            new ValidationProblemDetails(context.ModelState)
            {
                Title = "Request validation failed.",
                Status = StatusCodes.Status400BadRequest
            });
});

// ---------------------------------------------------------
// CORS for Angular frontend
// ---------------------------------------------------------

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ??
    [
        "http://localhost:4200"
    ];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------------------------------------------------------
// Build application
// ---------------------------------------------------------

var app = builder.Build();

// Forwarded headers must run before authentication,
// HTTPS handling and other request-processing middleware.
app.UseForwardedHeaders();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger is available only locally.
// Remove this condition if you need Swagger in production.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Railway handles public HTTPS at its reverse proxy.
// Therefore, UseHttpsRedirection is unnecessary inside
// the Railway container and may cause proxy redirect issues.
//
// app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program;