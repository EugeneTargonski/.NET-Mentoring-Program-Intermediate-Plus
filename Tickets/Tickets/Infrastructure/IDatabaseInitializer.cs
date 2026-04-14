namespace Tickets.Infrastructure;

/// <summary>
/// Responsibility: Initialize database instances
/// </summary>
public interface IDatabaseInitializer
{
    Task InitializeAsync();
}