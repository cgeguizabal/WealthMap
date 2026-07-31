using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WealthMap.Infrastructure.Persistence;

namespace WealthMap.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WealthMapDb")
            ?? throw new InvalidOperationException(
                "Connection string 'WealthMapDb' was not found. " +
                "Did you set it via user-secrets?");

        services.AddDbContext<WealthMapDbContext>(options =>
           options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

        return services;
    }
}