using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Infrastructure.Persistence.Configurations;



namespace WealthMap.Infrastructure.Persistence;

public class WealthMapDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();

    public DbSet<AccountMovement> AccountMovements => Set<AccountMovement>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<AdditionalIncome> AdditionalIncomes => Set<AdditionalIncome>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<InstallmentPurchase> InstallmentPurchases => Set<InstallmentPurchase>();
    public DbSet<Debt> Debts => Set<Debt>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<ProductGoal> ProductGoals => Set<ProductGoal>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SalaryDeposit> SalaryDeposits => Set<SalaryDeposit>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BankDefault> BankDefaults => Set<BankDefault>();

    /// <summary>
    /// The encryption service reaches the entity configurations through here.
    /// </summary>
    /// <remarks>
    /// EF builds the model once per context type, so the converters close over
    /// whatever instance was injected. The service is a stateless singleton, which
    /// is what makes that safe.
    /// </remarks>
    private readonly IEncryptionService _encryption;

    public WealthMapDbContext(
        DbContextOptions<WealthMapDbContext> options, IEncryptionService encryption)
        : base(options)
    {
        _encryption = encryption;
    }

    // DbSet<Account> Accounts, DbSet<CreditCard> CreditCards

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Everything that needs nothing from the container is still discovered
        // automatically. The predicate is what keeps the two groups apart:
        // ApplyConfigurationsFromAssembly builds each configuration with
        // Activator.CreateInstance and no arguments, so a configuration taking the
        // encryption service would throw if it were swept up here.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(WealthMapDbContext).Assembly,
            type => type.GetConstructor(Type.EmptyTypes) is not null);

        // The rest are handed the service explicitly. Listing them is the point:
        // this is the complete inventory of tables holding encrypted columns, and
        // adding one is a visible edit rather than an invisible convention.
        modelBuilder.ApplyConfiguration(new UserConfiguration(_encryption));
        modelBuilder.ApplyConfiguration(new AccountConfiguration(_encryption));
        modelBuilder.ApplyConfiguration(new CreditCardConfiguration(_encryption));
        modelBuilder.ApplyConfiguration(new DebtConfiguration(_encryption));
        modelBuilder.ApplyConfiguration(new PurchaseConfiguration(_encryption));
        modelBuilder.ApplyConfiguration(new SavingsGoalConfiguration(_encryption));
        modelBuilder.ApplyConfiguration(new ProductGoalConfiguration(_encryption));
        modelBuilder.ApplyConfiguration(new NotificationConfiguration(_encryption));

        GuardEveryEncryptedConfigurationIsApplied();

        // Ids come from BaseEntity (Guid.CreateVersion7()), never from the database.
        // Without this, EF treats aggregate children discovered via navigations
        // (e.g. a Deduction added to a tracked Job) as existing rows and issues
        // an UPDATE instead of an INSERT.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var id = entityType.FindProperty(nameof(Domain.Common.BaseEntity.Id));
            if (id is not null)
                id.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
        }
    }

    /// <summary>
    /// How many configurations are applied by hand just above.
    /// </summary>
    private const int EncryptedConfigurationCount = 8;

    /// <summary>
    /// Turns a silently skipped configuration into a startup failure.
    /// </summary>
    /// <remarks>
    /// EF warns when the assembly scan finds a configuration it cannot build, and
    /// that warning is suppressed in AddInfrastructure because eight of them are
    /// expected. The danger is the ninth: add a configuration that takes the
    /// encryption service, forget the ApplyConfiguration line, and the table would
    /// quietly lose its entire mapping with nothing logged.
    ///
    /// Counting is enough to catch it. The list above is the only way a
    /// constructor-taking configuration reaches the model, so any mismatch means
    /// one is missing from it.
    /// </remarks>
    private static void GuardEveryEncryptedConfigurationIsApplied()
    {
        var needingTheService = typeof(WealthMapDbContext).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract
                && type.GetInterfaces().Any(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                && type.GetConstructor(Type.EmptyTypes) is null)
            .Select(type => type.Name)
            .OrderBy(name => name)
            .ToList();

        if (needingTheService.Count != EncryptedConfigurationCount)
            throw new InvalidOperationException(
                $"{needingTheService.Count} entity configurations require the encryption " +
                $"service but {EncryptedConfigurationCount} are applied in OnModelCreating: " +
                $"{string.Join(", ", needingTheService)}. Add the missing " +
                "ApplyConfiguration call and update EncryptedConfigurationCount.");
    }

    /// <summary>
    /// Keeps <c>email_lookup</c> in step with <c>email</c> on every write.
    /// </summary>
    /// <remarks>
    /// Done here rather than in the repository so it cannot be forgotten. A user
    /// saved without its blind index would be invisible to sign-in and to the
    /// duplicate check — a silent failure, and the worst kind: registration would
    /// appear to succeed and the account would then not exist.
    /// </remarks>
    private void SyncEmailLookup()
    {
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;

            entry.Property(UserConfiguration.EmailLookup).CurrentValue =
                _encryption.BlindIndex(entry.Entity.Email);
        }
    }

    public override int SaveChanges()
    {
        SyncEmailLookup();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SyncEmailLookup();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}