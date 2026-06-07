using System.Linq.Expressions;
using Cure.Domain.Common;

namespace Cure.Domain.Services;

public interface IClinicalDataService
{
    Task<Result<T>> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : Entity;

    Task<Result<PagedResult<T>>> GetPagedAsync<T>(
        Expression<Func<T, bool>>? filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
        where T : Entity;

    Task<Result<T>> CreateAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : Entity;

    Task<Result<T>> UpdateAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : Entity;

    Task<Result> DeleteAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : Entity;
}
