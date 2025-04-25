using Core.Domain.Common;

namespace Application.Interface.Persistence
{
  public interface ISpecificationBuilder<TEntity> where TEntity : Entity
  {
    public ISpecification<TEntity> Build();
  }
}
