using Microsoft.EntityFrameworkCore;
using WealthMap.Domain.Entities;


namespace WealthMap.Infrastructure.Persistence;

public class WealthMapDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();

    public DbSet<AccountMovement> AccountMovements => Set<AccountMovement>();
    
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