using System.Linq.Expressions;
using Core.Domain.Common;

namespace Core.Interfaces.Persistence.GenericRepository
{
  public interface ISpecification<TEntity> where TEntity : Entity
  {
    /// <summary>
    /// Indicates whether the query should track changes.
    /// </summary>
    bool Tracking { get; }

    /// <summary>
    /// Criteria for filtering the query.
    /// </summary>
    Expression<Func<TEntity, bool>>? Criteria { get; }

    /// <summary>
    /// Strongly-typed navigation properties to include in the query.
    /// </summary>
    List<Expression<Func<TEntity, object>>> Includes { get; }

    /// <summary>
    /// String-based navigation properties to include in the query.
    /// </summary>
    List<string> IncludeString { get; }

    /// <summary>
    /// Maximum number of records to retrieve.
    /// </summary>
    int? Take { get; set; }

    /// <summary>
    /// Number of records to skip.
    /// </summary>
    int? Skip { get; set; }

    /// <summary>
    /// Indicates whether paging is enabled.
    /// </summary>
    bool IsPagingEnabled { get; set; }

    /// <summary>
    /// Applies paging to the query.
    /// </summary>
    /// <param name="take">The number of records to take.</param>
    /// <param name="skip">The number of records to skip.</param>
    void ApplyPaging(int take, int skip);
  }
}
