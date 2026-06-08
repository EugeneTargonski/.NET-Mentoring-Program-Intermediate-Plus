namespace CartCleanupWorker.Configuration;

/// <summary>
/// Settings for cart cleanup worker
/// </summary>
public class CartCleanupSettings
{
    public int IntervalSeconds { get; set; } = 60;
    public int CartExpirationMinutes { get; set; } = 15;
}
