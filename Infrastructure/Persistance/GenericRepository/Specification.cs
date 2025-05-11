using System.Linq.Expressions;
using Core.Domain.Common;
using Application.Interfaces.Persistence.GenericRepository;

namespace Infrastructure.Persistance.GenericRepository
{
  internal class Specification<TEntity> : ISpecification<TEntity> where TEntity : Entity
  {
    internal Specification() { }

    internal Specification(
      Expression<Func<TEntity, bool>> criteria,
      bool tracking = false)
    {
      Criteria = criteria;
      Tracking = tracking;
    }

    public bool Tracking { get; } = false;

    public Expression<Func<TEntity, bool>>? Criteria { get; } = null;

    //TODO multiples could be added and what then?
    public List<Expression<Func<TEntity, object>>> Includes { get; } = new();

    public List<string> IncludeString { get; } = new();

    public int? Take { get; set; } = null;

    public int? Skip { get; set; } = null;

    public bool IsPagingEnabled { get; set; } = false;

    public void AddInclude(Expression<Func<TEntity, object>> include)
    {
      if (include == null) throw new ArgumentNullException(nameof(include));
      Includes.Add(include);
    }

    void ISpecification<TEntity>.ApplyPaging(int take, int skip)
    {
      if (take <= 0) throw new ArgumentException("Take must be greater than zero.", nameof(take));
      if (skip < 0) throw new ArgumentException("Skip cannot be negative.", nameof(skip));

      Take = take;
      Skip = skip;
      IsPagingEnabled = true;
    }
  }
}
