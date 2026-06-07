using Cure.Domain.Common;
using Cure.Domain.Repositories;

namespace Cure.Application.Abstractions;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : Entity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
