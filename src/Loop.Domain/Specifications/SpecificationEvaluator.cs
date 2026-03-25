using SharedKernel.Interfaces;

namespace Domain.Specifications;

/// <summary>
/// Builds a LINQ <see cref="IQueryable{TEntity}"/> from a specification.
/// Criteria, ordering and paging are applied here (pure LINQ – no EF Core dependency).
/// <para>
/// <b>Includes</b> (eager loading) must be applied in the Infrastructure layer where
/// EF Core's <c>.Include()</c> / <c>.ThenInclude()</c> extension methods are available.
/// Simply iterate <see cref="ISpecification{T}.Includes"/> and
/// <see cref="ISpecification{T}.IncludeStrings"/> inside your repository implementation.
/// </para>
/// </summary>
public static class SpecificationEvaluator<TEntity> where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> spec)
    {
        IQueryable<TEntity> query = inputQuery;
        // 1. WHERE
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }

        // 2. ORDER BY
        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending is not null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // 3. PAGING  (must come after ordering)
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}
