using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WealthMap.Infrastructure.Persistence;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Infrastructure.Persistence.Repositories;
using WealthMap.Infrastructure.Auth;
using WealthMap.Infrastructure.Reports;
using WealthMap.Infrastructure.Security;



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

        // Registered before the DbContext: the context takes it as a constructor
        // dependency, and the entity configurations build their converters from it.
        services.Configure<EncryptionSettings>(
            configuration.GetSection(EncryptionSettings.SectionName));
        services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();

        services.AddDbContext<WealthMapDbContext>(options =>
           options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
           // The configurations that encrypt columns take the encryption
           // service in their constructor, so the assembly scan cannot build them
           // and logs this warning for each on every startup. They are applied by
           // hand immediately afterwards; the warning is noise, not news.
           // WealthMapDbContext.OnModelCreating guards the count so a new one
           // cannot go missing under the silence.
           .ConfigureWarnings(w =>
               w.Ignore(CoreEventId.SkippedEntityTypeConfigurationWarning)));

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
           services.AddScoped<IFreelanceJobRepository, FreelanceJobRepository>();
           services.AddScoped<ICardIncidentRepository, CardIncidentRepository>();
           services.AddScoped<IUserEraser, UserEraser>();

           // Resolved only by the --encrypt-pii command line flag, never by a
           // request. Registering it costs nothing; running it is deliberate.
           services.AddScoped<PiiEncryptionRunner>();

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