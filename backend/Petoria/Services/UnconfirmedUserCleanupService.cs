using Microsoft.AspNetCore.Identity;
using Petoria.Infrastructure.Data.Entities;

namespace Petoria.Services;

public sealed class UnconfirmedUserCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ConfirmationWindow = TimeSpan.FromMinutes(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UnconfirmedUserCleanupService> _logger;

    public UnconfirmedUserCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<UnconfirmedUserCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("UnconfirmedUserCleanupService started.");

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupExpiredUnconfirmedUsersAsync(stoppingToken);
        }
    }

    private async Task CleanupExpiredUnconfirmedUsersAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var cutoff = DateTime.UtcNow - ConfirmationWindow;

            var expiredUsers = userManager.Users
                .Where(u => !u.EmailConfirmed && u.CreatedAt < cutoff && u.CreatedAt != DateTime.MinValue)
                .ToList();

            if (expiredUsers.Count == 0)
                return;

            _logger.LogInformation(
                "Cleanup tick: deleting {Count} expired unconfirmed user(s).", expiredUsers.Count);

            foreach (var user in expiredUsers)
            {
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning(
                        "Failed to delete unconfirmed user {UserId}: {Errors}",
                        user.Id,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unhandled exception in UnconfirmedUserCleanupService.");
        }
    }
}
