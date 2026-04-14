using System.Linq.Expressions;
using Tickets.Domain.Entities;

namespace Tickets.Data.Abstractions;

/// <summary>
/// Database-agnostic repository interface
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryAsync(string queryString, string? partitionKey = null, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, string? partitionKey = null, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, string? partitionKey = null, CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> CreateBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, string partitionKey, CancellationToken cancellationToken = default);
    Task<int> DeleteBulkAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}