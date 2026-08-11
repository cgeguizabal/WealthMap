using WealthMap.Application.Common.Services;

namespace WealthMap.Api.BackgroundServices;

/// <summary>
/// Runs the salary catch-up on startup and once a day thereafter.
/// </summary>
/// <remarks>
/// The startup run is the important one. This app is not expected to be up
/// continuously, so a pure daily timer would silently skip any payday that fell
/// while it was stopped. Because the service settles every unpaid payday rather
/// than only today's, starting up after a week off pays that week in full.
/// </remarks>
public class SalaryPostingRunner : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceProvider _services;
    private readonly ILogger<SalaryPostingRunner> _logger;

    public SalaryPostingRunner(IServiceProvider services, ILogger<SalaryPostingRunner> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the app finish starting before touching the database.
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
            // The service is scoped; a background service is not.
            using var scope = _services.CreateScope();
            var poster = scope.ServiceProvider.GetRequiredService<SalaryPostingService>();

            var posted = await poster.PostAllDueAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct);

            if (posted > 0)
                _logger.LogInformation("Salary posting run complete: {Count} deposit(s) posted.", posted);
        }
        catch (OperationCanceledException)
        {
            // Shutting down.
        }
        catch (Exception ex)
        {
            // Never let a failed run kill the loop — tomorrow's run would be lost too,
            // and with it every payday until someone noticed.
            _logger.LogError(ex, "Salary posting run failed. It will be retried on the next run.");
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
