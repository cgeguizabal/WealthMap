using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<ISender, Sender>();

        // Find every class implementing IRequestHandler<,> and register it
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Implementation = t, Service = i }));

        foreach (var handler in handlerTypes)
            services.AddScoped(handler.Service, handler.Implementation);

        return services;
    }
}