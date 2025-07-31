using retoSquadmakers.Domain.Repositories;
using retoSquadmakers.Domain.Services;

namespace retoSquadmakers.Application.Services;

public abstract class BaseService<TEntity, TId> : IBaseService<TEntity, TId>
    where TEntity : class
    where TId : struct
{
    protected readonly IBaseRepository<TEntity, TId> _repository;

    protected BaseService(IBaseRepository<TEntity, TId> repository)
    {
        _repository = repository;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        await ValidateEntityAsync(entity, isUpdate: false);
        return await _repository.CreateAsync(entity);
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        await ValidateEntityAsync(entity, isUpdate: true);
        return await _repository.UpdateAsync(entity);
    }

    public virtual async Task DeleteAsync(TId id)
    {
        await ValidateDeleteAsync(id);
        await _repository.DeleteAsync(id);
    }

    public virtual async Task<bool> ExistsAsync(TId id)
    {
        return await _repository.ExistsAsync(id);
    }

    // Virtual methods para ser sobrescritos por servicios específicos
    protected virtual Task ValidateEntityAsync(TEntity entity, bool isUpdate)
    {
        // Implementación base - puede ser sobrescrita
        return Task.CompletedTask;
    }

    protected virtual Task ValidateDeleteAsync(TId id)
    {
        // Implementación base - puede ser sobrescrita
        return Task.CompletedTask;
    }
}