using Microsoft.EntityFrameworkCore;

namespace WealthMap.Infrastructure.Persistence;

public class WealthMapDbContext : DbContext
{
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
    }
}