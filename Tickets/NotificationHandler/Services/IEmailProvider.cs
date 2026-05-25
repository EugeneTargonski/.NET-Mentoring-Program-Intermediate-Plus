using NotificationHandler.Models;

namespace NotificationHandler.Services;

/// <summary>
/// Interface for email provider service
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Sends an email notification
    /// </summary>
    Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken = default);
}
