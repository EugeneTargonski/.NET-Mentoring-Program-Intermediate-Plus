using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Tickets.Tests.Integration;

/// <summary>
/// Base class for integration tests with common setup and helper methods
/// </summary>
public class IntegrationTestBase : IClassFixture<TicketsWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly TicketsWebApplicationFactory Factory;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IntegrationTestBase(TicketsWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public virtual Task InitializeAsync()
    {
        // Override in derived classes to set up test data
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        // Override in derived classes to clean up test data
        return Task.CompletedTask;
    }

    /// <summary>
    /// Helper method to send GET request and deserialize response
    /// </summary>
    protected async Task<T?> GetAsync<T>(string requestUri)
    {
        var response = await Client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    /// <summary>
    /// Helper method to send POST request with JSON body
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<T>(string requestUri, T content)
    {
        return await Client.PostAsJsonAsync(requestUri, content);
    }

    /// <summary>
    /// Helper method to send POST request and deserialize response
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest content)
    {
        var response = await Client.PostAsJsonAsync(requestUri, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    /// <summary>
    /// Helper method to send DELETE request
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
        return await Client.DeleteAsync(requestUri);
    }

    /// <summary>
    /// Helper method to send PATCH request
    /// </summary>
    protected async Task<HttpResponseMessage> PatchAsync(string requestUri)
    {
        return await Client.PatchAsync(requestUri, null);
    }

    /// <summary>
    /// Helper method to send PATCH request and deserialize response
    /// </summary>
    protected async Task<T?> PatchAsync<T>(string requestUri)
    {
        var response = await Client.PatchAsync(requestUri, null);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }
}
