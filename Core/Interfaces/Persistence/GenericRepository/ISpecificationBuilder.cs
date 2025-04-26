using Core.Domain.Common;

namespace Core.Interfaces.Persistence.GenericRepository
{
  public interface ISpecificationBuilder<TEntity> where TEntity : Entity
  {
    public ISpecification<TEntity> Build();
  }
}
