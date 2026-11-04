using Chats24h.Data;
using Microsoft.EntityFrameworkCore;

namespace Chats24h.Services;

public class RoomCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<RoomCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        try
        {
            do
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var db = scope.ServiceProvider
                        .GetRequiredService<ApplicationDbContext>();
                    var now = DateTimeOffset.UtcNow;

                    await db.ChatRooms
                        .Where(room => room.ExpiresAt <= now)
                        .ExecuteDeleteAsync(stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogError(ex, "Failed to delete expired chat rooms.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
    }
}
