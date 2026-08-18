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
    ///
    /// Matched against every candidate index rather than one, because a blind
    /// index carries no version stamp. While a key rotation is in flight the table
    /// holds indexes from both keys, and testing only the current one would report
    /// everyone the pass had not reached yet as not existing — every one of them
    /// locked out mid-rotation.
    /// </remarks>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var lookups = _encryption.BlindIndexCandidates(email);

        return await Set.FirstOrDefaultAsync(
            u => lookups.Contains(EF.Property<string>(u, UserConfiguration.EmailLookup)), ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var lookups = _encryption.BlindIndexCandidates(email);

        return await Set.AnyAsync(
            u => lookups.Contains(EF.Property<string>(u, UserConfiguration.EmailLookup)), ct);
    }
}
