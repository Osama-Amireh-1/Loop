using Application.Interfaces;
using Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Interfaces;

namespace Infrastructure.Reposiroty;

public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context;

    public ReadOnlyRepository(DbContext dbContext)
    {
        _context = dbContext;
    }

    public int Count(ISpecification<TEntity> spec)
    {
        IQueryable<TEntity> query = ApplySpecification(spec);
        return query.Count();
    }

    public Task<int> CountAsync(ISpecification<TEntity> spec)
    {
        IQueryable<TEntity> query = ApplySpecification(spec);
        return query.CountAsync();
    }

    public IQueryable<TEntity> Find(ISpecification<TEntity> spec)
    {
        return ApplySpecification(spec);
    }

    public IQueryable<TEntity> GetAll()
    {
        return _context.Set<TEntity>()
            .AsNoTracking()
            .AsQueryable();
    }

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
    {
        return SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsNoTracking().AsQueryable(), spec);

    }
}
