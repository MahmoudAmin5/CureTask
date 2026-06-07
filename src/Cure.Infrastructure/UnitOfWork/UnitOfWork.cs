using Cure.Application.Abstractions;
using Cure.Domain.Common;
using Cure.Domain.Repositories;
using Cure.Infrastructure.Data;
using Cure.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cure.Infrastructure.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly CureDbContext _context;
    private readonly Dictionary<Type, object> _repositories = [];
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(CureDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<T> Repository<T>() where T : Entity
    {
        var type = typeof(T);

        if (_repositories.TryGetValue(type, out var existingRepository))
        {
            return (IGenericRepository<T>)existingRepository;
        }

        var repository = new GenericRepository<T>(_context);
        _repositories[type] = repository;
        return repository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.CommitAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
