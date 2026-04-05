using Application.Interfaces;
using Domain.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Interfaces;

namespace Infrastructure.Reposiroty;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : AggregateRoot
{
    private readonly DbContext _context;

    public Repository(DbContext dbContext)
    {
        _context = dbContext;
    }

    public Task AddAsync(TEntity entity)
    {
        return _context.Set<TEntity>().AddAsync(entity).AsTask();
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        return _context.Set<TEntity>().AddRangeAsync(entities);
    }

    public void Add(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().AddRange(entities);
    }

    public Task<int> CountAsync(ISpecification<TEntity> spec)
    {
        IQueryable<TEntity> query = ApplySpecification(spec);
        return query.CountAsync();
    }

    public int Count(ISpecification<TEntity> spec)
    {
        IQueryable<TEntity> query = ApplySpecification(spec);
        return query.Count();
    }

    public IQueryable<TEntity> Find(ISpecification<TEntity> spec)
    {
        return ApplySpecification(spec);
    }

    public IQueryable<TEntity> GetAll()
    {
        return _context.Set<TEntity>().AsQueryable();
    }

    public void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public void DeleteRange(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().RemoveRange(entities);
    }

    public Task DeleteAsync(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        _context.Set<TEntity>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
    {
        return SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), spec);
    }
}
