using HotelReservation.Application.Interfaces.Services;

namespace HotelReservation.API.BackgroundServices;

public class ReservationCompletionBackgroundService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationCompletionBackgroundService>
        _logger;

    public ReservationCompletionBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationCompletionBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await CompleteReservationsAsync(
            stoppingToken);

        using var timer =
            new PeriodicTimer(
                TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(
                   stoppingToken))
        {
            await CompleteReservationsAsync(
                stoppingToken);
        }
    }

    private async Task CompleteReservationsAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await using AsyncServiceScope scope =
                _scopeFactory.CreateAsyncScope();

            IReservationCompletionService service =
                scope.ServiceProvider
                    .GetRequiredService<
                        IReservationCompletionService>();

            int completedCount =
                await service
                    .CompleteExpiredReservationsAsync();

            if (completedCount > 0)
            {
                _logger.LogInformation(
                    "{Count} reservation(s) automatically marked as completed.",
                    completedCount);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Application is shutting down.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error while automatically completing reservations.");
        }
    }
}