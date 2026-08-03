using WealthMap.Application.Common.Interfaces;

namespace WealthMap.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly WealthMapDbContext _context;

    public UnitOfWork(WealthMapDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            await operation();
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}