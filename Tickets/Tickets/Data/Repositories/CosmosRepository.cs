using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;

namespace Tickets.Data.Repositories;

/// <summary>
/// Cosmos DB implementation of IRepository
/// Implements the database-agnostic interface
/// </summary>
public class CosmosRepository<T>(Container container, ILogger<CosmosRepository<T>> logger) : IRepository<T> where T : BaseEntity
{
    protected readonly Container _container = container ?? throw new ArgumentNullException(nameof(container));
    protected readonly ILogger<CosmosRepository<T>> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public virtual async Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Getting {EntityType} with ID: {Id}, PartitionKey: {PartitionKey}", typeof(T).Name, id, partitionKey);
            }

            var response = await _container.ReadItemAsync<T>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("{EntityType} with ID: {Id} not found", typeof(T).Name, id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} with ID: {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Getting all {EntityType} entities", typeof(T).Name);
            }

            var query = _container.GetItemLinqQueryable<T>()
                .Where(x => x.EntityType == typeof(T).Name)
                .ToFeedIterator();

            var results = new List<T>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync(cancellationToken);
                results.AddRange(response);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Retrieved {Count} items, RU charge: {RUCharge}", response.Count, response.RequestCharge);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Querying {EntityType} with predicate", typeof(T).Name);
            }

            var queryable = _container.GetItemLinqQueryable<T>(
                requestOptions: partitionKey != null ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) } : null)
                .Where(x => x.EntityType == typeof(T).Name)
                .Where(predicate);

            var iterator = queryable.ToFeedIterator();

            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Retrieved {Count} items, RU charge: {RUCharge}", response.Count, response.RequestCharge);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> QueryAsync(string sqlQuery, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executing SQL query on {EntityType}: {Query}", typeof(T).Name, sqlQuery);
            }

            var queryDefinition = new QueryDefinition(sqlQuery);
            var queryRequestOptions = partitionKey != null ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) } : null;

            var iterator = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: queryRequestOptions);

            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Retrieved {Count} items, RU charge: {RUCharge}", response.Count, response.RequestCharge);
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL query on {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Getting first {EntityType} matching predicate", typeof(T).Name);
            }

            var results = await QueryAsync(predicate, partitionKey, cancellationToken);
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await QueryAsync(predicate, partitionKey, cancellationToken);
            return results.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<T> results;
            if (predicate == null)
            {
                results = await GetAllAsync(cancellationToken);
            }
            else
            {
                results = await QueryAsync(predicate, partitionKey, cancellationToken);
            }
            return results.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating new {EntityType} entity with ID: {Id}", typeof(T).Name, entity.Id);
            }

            entity.EntityType = typeof(T).Name;
            entity.CreatedAt = DateTime.UtcNow;

            var response = await _container.CreateItemAsync(
                entity,
                new PartitionKey(entity.PartitionKey),
                cancellationToken: cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully created {EntityType} entity, RU charge: {RUCharge}", typeof(T).Name, response.RequestCharge);
            }

            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType} entity", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> CreateBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            var entityList = entities.ToList();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating {Count} {EntityType} entities in bulk", entityList.Count, typeof(T).Name);
            }

            var tasks = new List<Task<ItemResponse<T>>>();

            foreach (var entity in entityList)
            {
                entity.EntityType = typeof(T).Name;
                entity.CreatedAt = DateTime.UtcNow;

                tasks.Add(_container.CreateItemAsync(
                    entity,
                    new PartitionKey(entity.PartitionKey),
                    cancellationToken: cancellationToken));
            }

            var responses = await Task.WhenAll(tasks);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                var totalRU = responses.Sum(r => r.RequestCharge);
                _logger.LogInformation("Successfully created {Count} {EntityType} entities, Total RU charge: {RUCharge}",
                    entityList.Count, typeof(T).Name, totalRU);
            }

            return responses.Select(r => r.Resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Updating {EntityType} entity with ID: {Id}", typeof(T).Name, entity.Id);
            }

            entity.UpdatedAt = DateTime.UtcNow;

            var response = await _container.ReplaceItemAsync(
                entity,
                entity.Id,
                new PartitionKey(entity.PartitionKey),
                cancellationToken: cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully updated {EntityType} entity, RU charge: {RUCharge}", typeof(T).Name, response.RequestCharge);
            }

            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} entity with ID: {Id}", typeof(T).Name, entity.Id);
            throw;
        }
    }

    public virtual async Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Upserting {EntityType} entity with ID: {Id}", typeof(T).Name, entity.Id);
            }

            entity.EntityType = typeof(T).Name;
            entity.UpdatedAt = DateTime.UtcNow;

            var response = await _container.UpsertItemAsync(
                entity,
                new PartitionKey(entity.PartitionKey),
                cancellationToken: cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully upserted {EntityType} entity, RU charge: {RUCharge}", typeof(T).Name, response.RequestCharge);
            }

            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting {EntityType} entity with ID: {Id}", typeof(T).Name, entity.Id);
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Deleting {EntityType} entity with ID: {Id}", typeof(T).Name, id);
            }

            await _container.DeleteItemAsync<T>(
                id,
                new PartitionKey(partitionKey),
                cancellationToken: cancellationToken);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully deleted {EntityType} entity with ID: {Id}", typeof(T).Name, id);
            }

            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("{EntityType} with ID: {Id} not found for deletion", typeof(T).Name, id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} entity with ID: {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<int> DeleteBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            var entityList = entities.ToList();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Deleting {Count} {EntityType} entities in bulk", entityList.Count, typeof(T).Name);
            }

            var tasks = new List<Task<bool>>();

            foreach (var entity in entityList)
            {
                tasks.Add(DeleteAsync(entity.Id, entity.PartitionKey, cancellationToken));
            }

            var results = await Task.WhenAll(tasks);
            var deletedCount = results.Count(r => r);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Successfully deleted {Count} {EntityType} entities", deletedCount, typeof(T).Name);
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bulk {EntityType} entities", typeof(T).Name);
            throw;
        }
    }
}