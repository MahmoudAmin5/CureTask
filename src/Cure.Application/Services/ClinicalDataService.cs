using System.Linq.Expressions;
using Cure.Application.Abstractions;
using Cure.Domain.Common;
using Cure.Domain.Errors;
using Cure.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Cure.Application.Services;

public sealed class ClinicalDataService : IClinicalDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ClinicalDataService> _logger;

    public ClinicalDataService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ClinicalDataService> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<T>> GetByIdAsync<T>(
        Guid id,
        CancellationToken cancellationToken = default) where T : Entity
    {
        var cacheKey = $"{typeof(T).Name}:{id}";
        var cached = await _cacheService.GetAsync<T>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return Result<T>.Success(cached);
        }

        var entity = await _unitOfWork.Repository<T>().GetByIdAsync(id, cancellationToken);

        if (entity is null)
        {
            return Result<T>.Failure(DomainErrors.General.NotFound(typeof(T).Name, id));
        }

        await _cacheService.SetAsync(cacheKey, entity, cancellationToken: cancellationToken);

        _logger.LogDebug("Cache miss for {CacheKey}, loaded from database", cacheKey);
        return Result<T>.Success(entity);
    }

    public async Task<Result<PagedResult<T>>> GetPagedAsync<T>(
        Expression<Func<T, bool>>? filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) where T : Entity
    {
        var cacheKey = $"{typeof(T).Name}:page:{page}:{pageSize}";

        
        if (filter is null)
        {
            var cached = await _cacheService.GetAsync<PagedResult<T>>(cacheKey, cancellationToken);
            if (cached is not null)
            {
                _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
                return Result<PagedResult<T>>.Success(cached);
            }
        }

        var repo = _unitOfWork.Repository<T>();
        var items = await repo.GetPagedAsync(filter, page, pageSize, cancellationToken);
        var totalCount = await repo.CountAsync(filter, cancellationToken);

        var pagedResult = new PagedResult<T>(items, totalCount, page, pageSize);

        if (filter is null)
        {
            await _cacheService.SetAsync(cacheKey, pagedResult, cancellationToken: cancellationToken);
        }

        return Result<PagedResult<T>>.Success(pagedResult);
    }

    public async Task<Result<T>> CreateAsync<T>(
        T entity,
        CancellationToken cancellationToken = default) where T : Entity
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _unitOfWork.Repository<T>().Add(entity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _cacheService.RemoveByPrefixAsync(typeof(T).Name, cancellationToken);

            _logger.LogInformation(
                "{EntityType} {EntityId} created successfully",
                typeof(T).Name,
                entity.Id);

            return Result<T>.Success(entity);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Unexpected error creating {EntityType}",
                typeof(T).Name);

            throw;
        }
    }

    public async Task<Result<T>> UpdateAsync<T>(
        T entity,
        CancellationToken cancellationToken = default) where T : Entity
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var existing = await _unitOfWork.Repository<T>()
                .GetByIdAsync(entity.Id, cancellationToken);

            if (existing is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<T>.Failure(DomainErrors.General.NotFound(typeof(T).Name, entity.Id));
            }

            entity.UpdatedAtUtc = DateTime.UtcNow;
            _unitOfWork.Repository<T>().Update(entity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _cacheService.RemoveByPrefixAsync(typeof(T).Name, cancellationToken);

            _logger.LogInformation(
                "{EntityType} {EntityId} updated successfully",
                typeof(T).Name,
                entity.Id);

            return Result<T>.Success(entity);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Unexpected error updating {EntityType} {EntityId}",
                typeof(T).Name,
                entity.Id);

            throw;
        }
    }

    public async Task<Result> DeleteAsync<T>(
        Guid id,
        CancellationToken cancellationToken = default) where T : Entity
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var repo = _unitOfWork.Repository<T>();
            var entity = await repo.GetByIdAsync(id, cancellationToken);

            if (entity is null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(DomainErrors.General.NotFound(typeof(T).Name, id));
            }

            repo.Remove(entity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _cacheService.RemoveByPrefixAsync(typeof(T).Name, cancellationToken);

            _logger.LogInformation(
                "{EntityType} {EntityId} deleted successfully",
                typeof(T).Name,
                id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Unexpected error deleting {EntityType} {EntityId}",
                typeof(T).Name,
                id);

            throw;
        }
    }
}
