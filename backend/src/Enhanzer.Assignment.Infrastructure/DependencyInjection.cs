using Enhanzer.Assignment.Application.Auth;
using Enhanzer.Assignment.Application.Locations;
using Enhanzer.Assignment.Application.PurchaseBills;
using Enhanzer.Assignment.Infrastructure.Data;
using Enhanzer.Assignment.Infrastructure.ExternalAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enhanzer.Assignment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ExternalApiOptions>(configuration.GetSection(ExternalApiOptions.SectionName));

        services.AddDbContext<AssignmentDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IPurchaseBillRepository, PurchaseBillRepository>();

        services.AddHttpClient<IExternalAuthClient, ExternalAuthClient>();

        return services;
    }
}
