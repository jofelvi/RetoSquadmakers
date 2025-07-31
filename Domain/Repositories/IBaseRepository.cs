namespace retoSquadmakers.Domain.Repositories;

public interface IBaseRepository<TEntity, TId> 
    where TEntity : class
    where TId : struct
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
    Task<bool> ExistsAsync(TId id);
    Task<int> CountAsync();
}