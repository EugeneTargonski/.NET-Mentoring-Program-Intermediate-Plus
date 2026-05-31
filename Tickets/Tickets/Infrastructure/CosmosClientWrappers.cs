using Microsoft.Azure.Cosmos;

namespace Tickets.Infrastructure;

/// <summary>
/// Typed wrappers for CosmosClient instances to enable proper DI registration and disposal
/// </summary>
public sealed class EventDbCosmosClient(CosmosClient client) : IDisposable
{
    public CosmosClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class InventoryDbCosmosClient(CosmosClient client) : IDisposable
{
    public CosmosClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class TransactionDbCosmosClient(CosmosClient client) : IDisposable
{
    public CosmosClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class TicketDbCosmosClient(CosmosClient client) : IDisposable
{
    public CosmosClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
