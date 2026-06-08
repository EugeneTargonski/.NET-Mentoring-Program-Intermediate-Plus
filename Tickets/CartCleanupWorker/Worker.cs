using CartCleanupWorker.Configuration;
using CartCleanupWorker.Services;
using Microsoft.Extensions.Options;

namespace CartCleanupWorker;

/// <summary>
/// Background service that periodically cleans up expired cart items
/// Releases seats that have been in cart for more than the configured expiration time
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ICartCleanupService _cleanupService;
    private readonly CartCleanupSettings _settings;

    public Worker(
        ILogger<Worker> logger,
        ICartCleanupService cleanupService,
        IOptions<CartCleanupSettings> settings)
    {
        _logger = logger;
        _cleanupService = cleanupService;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cart Cleanup Worker started at: {time}", DateTimeOffset.Now);
        _logger.LogInformation("Cleanup interval: {interval} seconds", _settings.IntervalSeconds);
        _logger.LogInformation("Cart expiration: {expiration} minutes", _settings.CartExpirationMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _cleanupService.CleanupExpiredCartsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during cart cleanup");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.IntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Cart Cleanup Worker stopped at: {time}", DateTimeOffset.Now);
    }
}
