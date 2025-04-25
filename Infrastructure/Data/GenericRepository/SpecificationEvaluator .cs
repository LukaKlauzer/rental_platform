using Application.RepositoryInterfaces;
using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.GenericRepository
{
  internal static class SpecificationEvaluator<TEntity> where TEntity : Entity
  {

    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> dbSet, ISpecification<TEntity> specification)
    {
      // Start with the base queryable
      var queryable = specification.Tracking
          ? dbSet.AsQueryable()
          : dbSet.AsNoTracking(); // Respect the Tracking flag in the specification

      // Apply Criteria (filtering)
      if (specification.Criteria != null)
        queryable = queryable.Where(specification.Criteria);

      // Apply Includes (navigation properties)
      queryable = specification.Includes
          .Aggregate(queryable, (current, include) => current.Include(include));

      queryable = specification.IncludeString
        .Aggregate(queryable, (current, include) => current.Include(include));

      if (specification.IsPagingEnabled)
        queryable = queryable
          .Skip(specification.Skip.HasValue ? specification.Skip.Value : 0)
          .Take(specification.Take.HasValue ? specification.Take.Value : 10);

      return queryable;

    }
  }
}
