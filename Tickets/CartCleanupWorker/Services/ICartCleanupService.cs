namespace CartCleanupWorker.Services;

/// <summary>
/// Service interface for cart cleanup operations
/// </summary>
public interface ICartCleanupService
{
    /// <summary>
    /// Cleans up expired cart items and releases associated seats
    /// </summary>
    Task CleanupExpiredCartsAsync(CancellationToken cancellationToken = default);
}
