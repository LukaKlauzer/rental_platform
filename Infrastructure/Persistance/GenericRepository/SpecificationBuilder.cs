using System.Linq.Expressions;
using Core.Domain.Common;
using Application.Interfaces.Persistence.GenericRepository;

namespace Infrastructure.Persistance.GenericRepository
{
  internal class SpecificationBuilder<TEntity> : ISpecificationBuilder<TEntity> where TEntity : Entity
  {
    ISpecification<TEntity> _specification = new Specification<TEntity>();
    public SpecificationBuilder(Expression<Func<TEntity, bool>> criteria, bool tracking = false)
    {
      if (criteria == null) throw new ArgumentNullException(nameof(criteria));
      _specification = new Specification<TEntity>(criteria, tracking);
    }
    public SpecificationBuilder<TEntity> AddIncludes(Expression<Func<TEntity, object>> include)
    {
      if (include == null) throw new ArgumentNullException(nameof(include));
      _specification.Includes.Add(include);
      return this;
    }
    public SpecificationBuilder<TEntity> AddMultipleIncludes(List<Expression<Func<TEntity, object>>> includes)
    {
      if (includes == null) throw new ArgumentNullException(nameof(includes));
      _specification.Includes.AddRange(includes);
      return this;
    }
    public SpecificationBuilder<TEntity> AddIncludeString(string includeString)
    {
      if (string.IsNullOrEmpty(includeString)) throw new ArgumentNullException(nameof(includeString));
      _specification.IncludeString.Add(includeString);
      return this;
    }
    public SpecificationBuilder<TEntity> AddMultipleIncludeString(List<string> includeStrings)
    {
      if (includeStrings == null) throw new ArgumentNullException(nameof(includeStrings));
      _specification.IncludeString.AddRange(includeStrings);
      return this;
    }

    public SpecificationBuilder<TEntity> SetSkip(int skip)
    {
      if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
      _specification.Skip = skip;
      if (_specification.Take == null || _specification.Take == 0)
        _specification.Take = 1;
      _specification.IsPagingEnabled = true;
      return this;
    }

    public SpecificationBuilder<TEntity> SetTake(int take)
    {
      if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));
      _specification.Take = take == 0 ? 1 : take;
      _specification.IsPagingEnabled = true;

      return this;
    }
    ISpecification<TEntity> ISpecificationBuilder<TEntity>.Build()
    {
      return _specification;
    }
  }
}
