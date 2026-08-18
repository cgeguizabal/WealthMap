using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WealthMap.Application.Common.Interfaces;

namespace WealthMap.Infrastructure.Persistence;

/// <summary>
/// Deletes every row belonging to one user, in an order the foreign keys accept.
/// </summary>
/// <remarks>
/// A single <c>DELETE FROM users</c> will not do it. Eight foreign keys reference
/// accounts and credit cards with ON DELETE RESTRICT — jobs, payments, purchases,
/// installment plans, freelance work, additional incomes and bank defaults — and
/// RESTRICT is checked the moment the row is touched, not at the end of the
/// statement like NO ACTION. Cascading from the user would try to remove an
/// account while those rows still pointed at it and fail.
///
/// The RESTRICT choices are right and should stay: an account referenced by a
/// salary or a recorded payment must not vanish underneath it. Deleting an
/// entire account is simply the one operation that has to unwind them in order.
///
/// Lives in Infrastructure because it is knowledge about the database's shape,
/// not about the domain. The domain has no concept of a table order.
/// </remarks>
public sealed class UserEraser : IUserEraser
{
    private readonly WealthMapDbContext _db;

    public UserEraser(WealthMapDbContext db) => _db = db;

    public async Task EraseAsync(Guid userId, CancellationToken ct = default)
    {
        // Order matters and is not alphabetical. Each group may only be removed
        // once nothing left points at it.

        // 1. Rows that reference a job. Deductions and payment days cascade from
        //    the job itself, but salary deposits also reference a movement, so
        //    they have to go before both.
        await _db.SalaryDeposits.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);

        // 2. Everything holding a RESTRICT reference to an account or a card.
        //    Installment payments cascade from their plan.
        await _db.Payments.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.Purchases.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.InstallmentPurchases.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.FreelanceJobs.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.AdditionalIncomes.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.BankDefaults.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.Jobs.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);

        // 3. The movement history, now that nothing refers to it.
        await _db.AccountMovements.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);

        // 4. Records that only ever pointed at the user, plus goals — a savings
        //    goal's account link is SET NULL, so it could go either side of this.
        await _db.SavingsGoals.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.ProductGoals.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.Debts.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.Notifications.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.RefreshTokens.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);

        // 5. The instruments themselves.
        await _db.Accounts.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);
        await _db.CreditCards.Where(x => x.UserId == userId).ExecuteDeleteAsync(ct);

        // 6. Stores are deliberately not deleted. They are a shared catalogue and
        //    other users' purchases point at them; `created_by_user_id` is SET NULL
        //    by the foreign key, so the entry survives without naming anyone.

        await _db.Users.Where(x => x.Id == userId).ExecuteDeleteAsync(ct);

        await GuardNothingRemainsAsync(userId, ct);
    }

    /// <summary>
    /// Fails the transaction if any row anywhere still belongs to the user.
    /// </summary>
    /// <remarks>
    /// The list above is hand-ordered, so the realistic way it breaks is someone
    /// adding a table and not adding a line here — which would leave orphaned
    /// personal data behind while the API reported a successful deletion. That is
    /// the worst possible failure for this particular operation, and the exact
    /// promise the privacy policy makes.
    ///
    /// So the check does not read the list. It asks the database which tables have
    /// a user_id column and counts rows in each, which means a table nobody
    /// remembered is still caught. Throwing rolls the transaction back and leaves
    /// the account intact rather than half-erased.
    /// </remarks>
    private async Task GuardNothingRemainsAsync(Guid userId, CancellationToken ct)
    {
        var connection = _db.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.Transaction = _db.Database.CurrentTransaction?.GetDbTransaction();

        // Builds one query per table that has a user_id, then reports any with
        // rows left. All identifiers come from the catalogue, never from input.
        command.CommandText = """
            SELECT string_agg(
                       format('SELECT %L AS t, count(*) AS n FROM %I WHERE user_id = %L',
                              c.relname, c.relname, @userId::text),
                       ' UNION ALL ')
            FROM   pg_class c
            JOIN   pg_namespace n ON n.oid = c.relnamespace
            JOIN   pg_attribute a ON a.attrelid = c.oid
            WHERE  n.nspname = 'public'
              AND  c.relkind = 'r'
              AND  a.attname = 'user_id'
              AND  a.attnum > 0
              AND  NOT a.attisdropped;
            """;

        var idParameter = command.CreateParameter();
        idParameter.ParameterName = "userId";
        idParameter.Value = userId.ToString();
        command.Parameters.Add(idParameter);

        if (await command.ExecuteScalarAsync(ct) is not string union || union.Length == 0) return;

        await using var counts = connection.CreateCommand();
        counts.Transaction = command.Transaction;
        counts.CommandText = $"SELECT t, n FROM ({union}) totals WHERE n > 0";

        var leftovers = new List<string>();

        await using (var reader = await counts.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
                leftovers.Add($"{reader.GetString(0)} ({reader.GetInt64(1)})");
        }

        if (leftovers.Count > 0)
            throw new InvalidOperationException(
                $"Account deletion left rows behind in: {string.Join(", ", leftovers)}. " +
                "A table was added without being added to UserEraser. Nothing was deleted.");
    }
}
