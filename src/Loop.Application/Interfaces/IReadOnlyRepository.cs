using System;
using System.Collections.Generic;
using System.Text;
using Loop.SharedKernel;
using Loop.SharedKernel.Interfaces;

namespace Loop.Application.Interfaces;

public interface IReadOnlyRepository<TEntity> where TEntity : Entity
{
    IQueryable<TEntity> Find(ISpecification<TEntity> spec);
    Task<int> CountAsync(ISpecification<TEntity> spec);
    IQueryable<TEntity> GetAll();
    int Count(ISpecification<TEntity> spec);
}


