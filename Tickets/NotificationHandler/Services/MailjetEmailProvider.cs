using System.Net.Http.Json;

namespace NotificationHandler.Services;

/// <summary>
/// Email provider implementation using Mailjet API
/// API Documentation: https://dev.mailjet.com/email/guides/send-api-v31/
/// </summary>
public class MailjetEmailProvider : IEmailProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public MailjetEmailProvider(
        HttpClient httpClient,
        string fromEmail,
        string fromName)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _fromEmail = fromEmail ?? throw new ArgumentNullException(nameof(fromEmail));
        _fromName = fromName ?? throw new ArgumentNullException(nameof(fromName));
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        CancellationToken cancellationToken = default)
    {
        // Mailjet API v3.1 payload structure
        var payload = new
        {
            Messages = new[]
            {
                new
                {
                    From = new
                    {
                        Email = _fromEmail,
                        Name = _fromName
                    },
                    To = new[]
                    {
                        new
                        {
                            Email = toEmail,
                            Name = toName
                        }
                    },
                    Subject = subject,
                    HTMLPart = htmlContent,
                    TextPart = StripHtml(htmlContent)
                }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "send",
                payload,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"✓ Email sent successfully to {toEmail} via Mailjet");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"✗ Mailjet API error. Status: {response.StatusCode}");
                Console.WriteLine($"  Error: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Exception sending email via Mailjet: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Simple HTML tag stripper for plain text version
    /// </summary>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Remove HTML tags
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);

        // Decode HTML entities
        text = System.Net.WebUtility.HtmlDecode(text);

        // Clean up whitespace
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

        return text.Trim();
    }
}
