using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Common;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WealthMapDbContext Context;
    protected readonly DbSet<T> Set;

    protected Repository(WealthMapDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public virtual void Update(T entity) => Set.Update(entity);

    public virtual void Remove(T entity) => Set.Remove(entity);
}