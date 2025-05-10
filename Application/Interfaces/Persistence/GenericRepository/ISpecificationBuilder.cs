using Core.Domain.Common;

namespace Application.Interfaces.Persistence.GenericRepository
{
  public interface ISpecificationBuilder<TEntity> where TEntity : Entity
  {
    public ISpecification<TEntity> Build();
  }
}
