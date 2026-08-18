using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WealthMap.Application.Common.Interfaces;
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
///
/// One step cannot work that way. users.email_lookup has to be seeded with raw
/// SQL first, because EF cannot read a users row until it holds a value — see
/// SeedEmailLookupAsync. Everything after that, users included, goes through the
/// model.
///
/// Re-running is a genuine no-op, not merely a harmless one: each table is asked
/// which rows still lack the v1: envelope, and an already-converted database
/// returns none. That matters because the realistic failure is an interrupted
/// run, and the operator's instinct is to run it again.
/// </remarks>
public sealed class PiiEncryptionRunner
{
    private readonly WealthMapDbContext _db;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<PiiEncryptionRunner> _logger;

    public PiiEncryptionRunner(
        WealthMapDbContext db,
        IEncryptionService encryption,
        ILogger<PiiEncryptionRunner> logger)
    {
        _db = db;
        _encryption = encryption;
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
            // Must come first, and must not go through EF. See the method below.
            new("users.email_lookup", await SeedEmailLookupAsync(ct)),

            // users next: everything else is meaningless without an account that
            // can still sign in, so if this is going to fail, fail here.
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
    /// Fills <c>users.email_lookup</c> with raw SQL, before EF touches the table.
    /// </summary>
    /// <remarks>
    /// This exists because of a mismatch the rest of the runner does not have.
    /// The EF model describes the *finished* state, where email_lookup is NOT
    /// NULL; this runner executes in the *intermediate* state, where the column
    /// exists and is empty. Loading a User through the model therefore fails on
    /// materialisation — Npgsql is asked for a non-nullable string and finds NULL
    /// — before EF can write the value that would fix it.
    ///
    /// So the blind index cannot be bootstrapped through a model that already
    /// assumes it exists. It is seeded here instead, then every other table
    /// (users included) goes through EF as normal.
    ///
    /// The email read here may be plaintext on a first run or ciphertext on a
    /// re-run after an interruption. Decrypt handles both — it passes unprefixed
    /// values through untouched — so the index is always computed from the
    /// plaintext address, and always comes out the same.
    /// </remarks>
    private async Task<int> SeedEmailLookupAsync(CancellationToken ct)
    {
        var connection = _db.Database.GetDbConnection();
        await _db.Database.OpenConnectionAsync(ct);

        try
        {
            // Read fully before writing: Npgsql allows one active reader per
            // connection, so updating inside the loop would fail.
            var pending = new List<(Guid Id, string Email)>();

            await using (var read = connection.CreateCommand())
            {
                read.CommandText = "SELECT id, email FROM users WHERE email_lookup IS NULL";

                await using var reader = await read.ExecuteReaderAsync(ct);

                while (await reader.ReadAsync(ct))
                    pending.Add((reader.GetGuid(0), reader.GetString(1)));
            }

            if (pending.Count == 0)
            {
                _logger.LogInformation("users.email_lookup: already seeded.");
                return 0;
            }

            _logger.LogInformation(
                "users.email_lookup: seeding {Count} row(s).", pending.Count);

            foreach (var (id, email) in pending)
            {
                await using var write = connection.CreateCommand();
                write.CommandText = "UPDATE users SET email_lookup = @lookup WHERE id = @id";

                var lookup = write.CreateParameter();
                lookup.ParameterName = "lookup";
                lookup.Value = _encryption.BlindIndex(_encryption.Decrypt(email));
                write.Parameters.Add(lookup);

                var key = write.CreateParameter();
                key.ParameterName = "id";
                key.Value = id;
                write.Parameters.Add(key);

                await write.ExecuteNonQueryAsync(ct);
            }

            return pending.Count;
        }
        finally
        {
            await _db.Database.CloseConnectionAsync();
        }
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
