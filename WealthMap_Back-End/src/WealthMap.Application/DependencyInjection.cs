using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using WealthMap.Application.Common.Behaviors;
using WealthMap.Application.Common.Messaging;

namespace WealthMap.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<ISender, Sender>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<Common.Services.FinancialSnapshotLoader>();
        services.AddScoped<Common.Services.SalaryPostingService>();
        services.AddScoped<Common.Services.SessionIssuer>();
        services.AddScoped<Common.Services.CardStatementLoader>();
        services.AddScoped<Common.Services.InstallmentContextLoader>();

        services.RegisterImplementationsOf(assembly, typeof(IRequestHandler<,>));
        services.RegisterImplementationsOf(assembly, typeof(IValidator<>));
        

        return services;
    }

    private static void RegisterImplementationsOf(
        this IServiceCollection services,
        Assembly assembly,
        Type openGenericInterface)
    {
        var registrations = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == openGenericInterface)
                .Select(i => new { Implementation = t, Service = i }));

        foreach (var item in registrations)
            services.AddScoped(item.Service, item.Implementation);
    }
}