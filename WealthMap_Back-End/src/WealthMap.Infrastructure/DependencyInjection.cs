using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WealthMap.Infrastructure.Persistence;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Infrastructure.Persistence.Repositories;
using WealthMap.Infrastructure.Auth;
using WealthMap.Infrastructure.Reports;



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

           services.AddScoped<IUnitOfWork, UnitOfWork>();
           services.AddScoped<IAccountRepository, AccountRepository>();
           services.AddScoped<IAccountMovementRepository, AccountMovementRepository>();
           services.AddScoped<ICreditCardRepository, CreditCardRepository>();
           services.AddScoped<IJobRepository, JobRepository>();
           services.AddScoped<IAdditionalIncomeRepository, AdditionalIncomeRepository>();
           services.AddScoped<IStoreRepository, StoreRepository>();
           services.AddScoped<IPurchaseRepository, PurchaseRepository>();
           services.AddScoped<IInstallmentPurchaseRepository, InstallmentPurchaseRepository>();
           services.AddScoped<IDebtRepository, DebtRepository>();
           services.AddScoped<ISavingsGoalRepository, SavingsGoalRepository>();
           services.AddScoped<IProductGoalRepository, ProductGoalRepository>();
           services.AddScoped<INotificationRepository, NotificationRepository>();
           services.AddScoped<IPaymentRepository, PaymentRepository>();
           services.AddScoped<ISalaryDepositRepository, SalaryDepositRepository>();
           services.AddScoped<IUserRepository, UserRepository>();
           services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
           services.AddScoped<IBankDefaultRepository, BankDefaultRepository>();

           // QuestPDF's Community licence covers this project; it must be set before any render.
           QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
           services.AddSingleton<IPdfReportGenerator, MonthlyReportPdfGenerator>();
           services.AddSingleton<IPasswordHasher, PasswordHasher>();
           
           services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
           services.AddSingleton<IJwtService, JwtService>();
           services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
        return services;
    }
}