using System;
using System.Collections.Generic;
using System.Text;
using SharedKernel.Interfaces;

namespace Application.Interfaces;

public interface IReadOnlyRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Find(ISpecification<TEntity> spec);
    Task<int> CountAsync(ISpecification<TEntity> spec);
    IQueryable<TEntity> GetAll();
    int Count(ISpecification<TEntity> spec);
}
