using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WealthMap.Domain.Common;

namespace WealthMap.Infrastructure.Persistence;

/// <summary>
/// Rewrites the rows that existed before encryption, once.
/// </summary>
/// <remarks>
/// The schema migration only widened the columns; every value in them is still
/// plaintext at that point, and users.email_lookup is empty. This is the pass
/// that converts them.
///
/// It does the work through EF rather than SQL on purpose. Loading an entity and
/// saving it unchanged runs the value converters in both directions — read
/// decrypts (a no-op on plaintext, by design), write encrypts — so the column
/// list lives in the entity configurations and nowhere else. A hand-written SQL
/// version would be a second copy of that list, free to drift from the first.
/// Saving users also fills email_lookup, because SaveChanges syncs it.
///
/// Re-running is a genuine no-op, not merely a harmless one: each table is asked
/// which rows still lack the v1: envelope, and an already-converted database
/// returns none. That matters because the realistic failure is an interrupted
/// run, and the operator's instinct is to run it again.
/// </remarks>
public sealed class PiiEncryptionRunner
{
    private readonly WealthMapDbContext _db;
    private readonly ILogger<PiiEncryptionRunner> _logger;

    public PiiEncryptionRunner(WealthMapDbContext db, ILogger<PiiEncryptionRunner> logger)
    {
        _db = db;
        _logger = logger;
    }

    public sealed record TableResult(string Table, int RowsEncrypted);

    /// <summary>
    /// Rows are saved in batches so a large table does not build one enormous
    /// transaction, and so an interruption leaves whole rows behind rather than
    /// half-converted ones.
    /// </summary>
    private const int BatchSize = 200;

    public async Task<IReadOnlyList<TableResult>> RunAsync(CancellationToken ct = default)
    {
        var results = new List<TableResult>
        {
            // users first: everything else is meaningless without an account that
            // can still sign in, so if the blind index is going to fail, fail here.
            new("users", await EncryptAsync(
                _db.Users, "users", "email NOT LIKE 'v1:%' OR email_lookup IS NULL", ct)),

            new("accounts", await EncryptAsync(
                _db.Accounts, "accounts", "name NOT LIKE 'v1:%'", ct)),

            new("credit_cards", await EncryptAsync(
                _db.CreditCards, "credit_cards", "card_name NOT LIKE 'v1:%'", ct)),

            new("debts", await EncryptAsync(
                _db.Debts, "debts", "name NOT LIKE 'v1:%'", ct)),

            // The only encrypted column here is nullable, so rows that never had a
            // note need no visit at all.
            new("purchases", await EncryptAsync(
                _db.Purchases, "purchases", "notes IS NOT NULL AND notes NOT LIKE 'v1:%'", ct)),

            new("savings_goals", await EncryptAsync(
                _db.SavingsGoals, "savings_goals", "name NOT LIKE 'v1:%'", ct)),

            new("product_goals", await EncryptAsync(
                _db.ProductGoals, "product_goals", "name NOT LIKE 'v1:%'", ct)),

            new("notifications", await EncryptAsync(
                _db.Notifications, "notifications", "title NOT LIKE 'v1:%'", ct))
        };

        return results;
    }

    /// <summary>
    /// Encrypts every row of one table that still needs it.
    /// </summary>
    /// <remarks>
    /// Marking an unmodified entity as Modified is what triggers the write. There
    /// is nothing to change on the entity itself — the plaintext it holds is
    /// already correct — and the whole point is to re-persist it through the
    /// converters.
    /// </remarks>
    private async Task<int> EncryptAsync<TEntity>(
        DbSet<TEntity> set,
        string table,
        string stillPlaintext,
        CancellationToken ct)
        where TEntity : BaseEntity
    {
        var pending = await PendingIdsAsync(table, stillPlaintext, ct);

        if (pending.Count == 0)
        {
            _logger.LogInformation("{Table}: nothing to encrypt.", table);
            return 0;
        }

        _logger.LogInformation("{Table}: encrypting {Count} row(s).", table, pending.Count);

        foreach (var batch in pending.Chunk(BatchSize))
        {
            var entities = await set.Where(e => batch.Contains(e.Id)).ToListAsync(ct);

            foreach (var entity in entities)
                _db.Entry(entity).State = EntityState.Modified;

            await _db.SaveChangesAsync(ct);

            // Nothing from this batch is needed again, and holding thousands of
            // tracked entities would slow every later batch down.
            _db.ChangeTracker.Clear();
        }

        return pending.Count;
    }

    /// <summary>
    /// Asks the database which rows are still plaintext.
    /// </summary>
    /// <remarks>
    /// Raw ADO because this is the one question EF cannot answer: reading through
    /// the model decrypts on the way out, so an encrypted row and a plaintext one
    /// look identical by the time LINQ sees them. The envelope prefix is only
    /// visible in the stored value.
    ///
    /// The interpolated fragments are compile-time constants from RunAsync above —
    /// no caller supplies them, and none of them ever touches user input.
    /// </remarks>
    private async Task<List<Guid>> PendingIdsAsync(
        string table, string stillPlaintext, CancellationToken ct)
    {
        var ids = new List<Guid>();

        var connection = _db.Database.GetDbConnection();
        await _db.Database.OpenConnectionAsync(ct);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT id FROM {table} WHERE {stillPlaintext}";

            await using var reader = await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
                ids.Add(reader.GetGuid(0));
        }
        finally
        {
            await _db.Database.CloseConnectionAsync();
        }

        return ids;
    }
}
