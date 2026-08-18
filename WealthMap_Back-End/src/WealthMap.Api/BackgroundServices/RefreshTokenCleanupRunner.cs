using WealthMap.Application.Common.Interfaces;

namespace WealthMap.Api.BackgroundServices;

/// <summary>
/// Deletes long-expired refresh tokens, on startup and once a day thereafter.
/// </summary>
/// <remarks>
/// Every sign-in writes a row, and every refresh writes another — rotation means
/// a single active session produces a row roughly every fifteen minutes of use.
/// Nothing was removing them, so the table grew without bound for the lifetime of
/// the app. It is the only table here that does that: everything else is bounded
/// by what the user actually records.
/// </remarks>
public class RefreshTokenCleanupRunner : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    /// <summary>
    /// Staggered behind <see cref="SalaryPostingRunner"/> so the two do not open
    /// their first connections at the same moment on a cold start.
    /// </summary>
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long an expired token is kept before deletion.
    /// </summary>
    /// <remarks>
    /// Not zero, deliberately. A token that has expired is already useless for
    /// authentication — <c>ExpiresAt</c> is checked on every refresh — so keeping
    /// it buys no access. What it buys is evidence: replay detection distinguishes
    /// "this token was rotated away" from "no such token", and that distinction
    /// disappears once the row is gone. Thirty days past expiry, on a fourteen-day
    /// token, leaves roughly six weeks of history to investigate an incident with
    /// while still bounding the table.
    /// </remarks>
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    private readonly IServiceProvider _services;
    private readonly ILogger<RefreshTokenCleanupRunner> _logger;

    public RefreshTokenCleanupRunner(
        IServiceProvider services, ILogger<RefreshTokenCleanupRunner> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(Interval);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await SafeWaitAsync(timer, stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            // The repository is scoped; a background service is not.
            using var scope = _services.CreateScope();
            var tokens = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

            var deleted = await tokens.DeleteExpiredAsync(DateTime.UtcNow - Retention, ct);

            if (deleted > 0)
                _logger.LogInformation("Removed {Count} expired refresh token(s).", deleted);
        }
        catch (OperationCanceledException)
        {
            // Shutting down.
        }
        catch (Exception ex)
        {
            // Never let a failed run kill the loop. Housekeeping falling behind is
            // a table that grows; the loop dying is a table that never stops.
            _logger.LogError(ex, "Refresh token cleanup failed. It will be retried on the next run.");
        }
    }

    private static async Task<bool> SafeWaitAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
