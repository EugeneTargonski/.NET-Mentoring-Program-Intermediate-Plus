using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tickets.Infrastructure;

/// <summary>
/// Action filter that adds HTTP caching support with ETag validation
/// Implements client-side caching with 304 Not Modified responses
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class HttpCacheAttribute : ActionFilterAttribute
{
    private readonly int _durationSeconds;
    private readonly bool _varyByQueryKeys;

    /// <summary>
    /// Creates HTTP cache attribute with specified duration
    /// </summary>
    /// <param name="durationSeconds">Cache duration in seconds (default: 60)</param>
    /// <param name="varyByQueryKeys">Whether to vary cache by query string parameters (default: false)</param>
    public HttpCacheAttribute(int durationSeconds = 60, bool varyByQueryKeys = false)
    {
        _durationSeconds = durationSeconds;
        _varyByQueryKeys = varyByQueryKeys;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        var response = context.HttpContext.Response;

        // Generate cache key based on path and optionally query string
        var cacheKey = GenerateCacheKey(request);

        // Check if client sent If-None-Match header with ETag
        var clientETag = request.Headers.IfNoneMatch.FirstOrDefault();

        // Execute the action
        var executedContext = await next();

        // Only apply caching for successful responses
        if (executedContext.Result is ObjectResult objectResult && 
            objectResult.StatusCode is null or >= 200 and < 300)
        {
            var responseData = objectResult.Value;

            // Generate ETag from response content
            var etag = GenerateETag(responseData);

            // Check if client's ETag matches current content
            if (clientETag != null && clientETag.Trim('"') == etag)
            {
                // Content hasn't changed - return 304 Not Modified
                executedContext.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
                executedContext.HttpContext.Response.Headers.ETag = $"\"{etag}\"";
                return;
            }

            // Set cache headers
            response.Headers.CacheControl = $"public, max-age={_durationSeconds}";
            response.Headers.ETag = $"\"{etag}\"";
            response.Headers.Vary = "Accept-Encoding";

            if (_varyByQueryKeys)
            {
                response.Headers.Vary = "Accept-Encoding, Accept";
            }

            // Add Last-Modified header (use current time as we don't track actual modification times)
            response.Headers.LastModified = DateTime.UtcNow.ToString("R");
        }
    }

    private string GenerateCacheKey(HttpRequest request)
    {
        var keyBuilder = new StringBuilder();
        keyBuilder.Append(request.Path);

        if (_varyByQueryKeys && request.QueryString.HasValue)
        {
            keyBuilder.Append(request.QueryString.Value);
        }

        return keyBuilder.ToString();
    }

    private string GenerateETag(object? data)
    {
        if (data == null)
            return string.Empty;

        // Serialize to JSON for consistent hashing
        var json = JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes(json);

        // Generate SHA256 hash
        var hash = SHA256.HashData(bytes);

        // Convert to hex string (shortened for efficiency)
        return Convert.ToHexString(hash)[..16];
    }
}
