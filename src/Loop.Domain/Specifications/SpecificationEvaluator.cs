using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel.Interfaces;

namespace Loop.Domain.Specifications;

/// <summary>
/// Builds a LINQ <see cref="IQueryable{TEntity}"/> from a specification.
/// Criteria, includes, ordering and paging are applied here.
/// </summary>
public static class SpecificationEvaluator<TEntity> where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> spec)
    {
        IQueryable<TEntity> query = inputQuery;
        
        // 1. INCLUDES (eager loading)
        query = spec.Includes.Aggregate(query, (current, includeExpression) =>
            current.Include(includeExpression));

        query = spec.IncludeStrings.Aggregate(query, (current, includeString) =>
            current.Include(includeString));

        // 2. WHERE
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        // 3. ORDER BY
        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending is not null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // 4. PAGING  (must come after ordering)
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}

