


using SharedKernel.Interfaces;

namespace Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<int> CountAsync(ISpecification<TEntity> spec);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    Task DeleteAsync(TEntity entity);
    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
    IQueryable<TEntity> Find(ISpecification<TEntity> spec);
    IQueryable<TEntity> GetAll();
    int Count(ISpecification<TEntity> spec);
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);
}
