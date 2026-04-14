namespace Tickets.Domain.Enums;

/// <summary>
/// Seat status based on State Machine Diagram
/// </summary>
public enum SeatStatus
{
    /// <summary>
    /// Seat is ready for purchase, visible to customers
    /// </summary>
    Available = 0,

    /// <summary>
    /// Temporary reservation (10-15 minutes), not visible to other customers
    /// </summary>
    OnHold = 1,

    /// <summary>
    /// Payment processing in progress, seat reserved but ticket not issued
    /// </summary>
    Booked = 2,

    /// <summary>
    /// Payment complete, ticket issued, cannot be purchased by others
    /// </summary>
    Sold = 3,

    /// <summary>
    /// Administratively restricted, not available for purchase (maintenance, VIP hold)
    /// </summary>
    Blocked = 4
}