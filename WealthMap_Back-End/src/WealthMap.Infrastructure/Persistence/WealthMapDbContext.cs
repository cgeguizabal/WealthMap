using Microsoft.EntityFrameworkCore;
using WealthMap.Domain.Entities;



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

    public WealthMapDbContext(DbContextOptions<WealthMapDbContext> options)
        : base(options)
    {
    }

    // DbSet<Account> Accounts, DbSet<CreditCard> CreditCards

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically applies every IEntityTypeConfiguration<T> in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WealthMapDbContext).Assembly);

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
}