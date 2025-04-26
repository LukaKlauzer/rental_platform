using Core.Domain.Common;

namespace Core.Interfaces.Persistence.GenericRepository
{
  public interface IRepository<TEntity> where TEntity : Entity
  {
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAsync(ISpecificationBuilder<TEntity> specificationBuilder, CancellationToken cancellationToken = default);
    Task<TEntity?> GetFirstAsync(ISpecificationBuilder<TEntity> specificationBuilder, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(ISpecificationBuilder<TEntity> specificationBuilder, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id);
  }
}
