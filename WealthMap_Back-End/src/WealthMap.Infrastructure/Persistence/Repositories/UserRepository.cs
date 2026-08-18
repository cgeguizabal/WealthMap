using Microsoft.EntityFrameworkCore;
using WealthMap.Application.Common.Interfaces;
using WealthMap.Domain.Entities;
using WealthMap.Infrastructure.Persistence.Configurations;

namespace WealthMap.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly IEncryptionService _encryption;

    public UserRepository(WealthMapDbContext context, IEncryptionService encryption)
        : base(context) => _encryption = encryption;

    /// <summary>
    /// Matched on the blind index, never on <c>email</c>.
    /// </summary>
    /// <remarks>
    /// The encrypted column cannot be compared: two encryptions of one address
    /// produce different ciphertext, so <c>WHERE email = …</c> would match nothing
    /// and every sign-in would fail with "invalid email or password".
    ///
    /// BlindIndex applies the same trim-and-lowercase as User.NormalizeEmail. If
    /// those two ever diverge the lookup misses silently, which is why the service
    /// documents the coupling rather than leaving it to be rediscovered.
    /// </remarks>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var lookup = _encryption.BlindIndex(email);

        return await Set.FirstOrDefaultAsync(
            u => EF.Property<string>(u, UserConfiguration.EmailLookup) == lookup, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var lookup = _encryption.BlindIndex(email);

        return await Set.AnyAsync(
            u => EF.Property<string>(u, UserConfiguration.EmailLookup) == lookup, ct);
    }
}
