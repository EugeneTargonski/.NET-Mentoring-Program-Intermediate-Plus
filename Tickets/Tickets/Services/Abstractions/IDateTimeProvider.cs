namespace Tickets.Services.Abstractions;

/// <summary>
/// Abstraction for date/time operations (DIP compliance)
/// Enables time control in unit tests
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
