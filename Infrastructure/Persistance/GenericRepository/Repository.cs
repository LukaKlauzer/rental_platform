using Core.Domain.Common;
using Core.Interfaces.Persistence.GenericRepository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.GenericRepository
{
  internal class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
  {
    private readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _set;

    public Repository(ApplicationDbContext context)
    {
      _context = context;
      _set = _context.Set<TEntity>();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
      await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) =>
      await _context.Database.BeginTransactionAsync(cancellationToken);


    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) =>
      await _context.Database.CommitTransactionAsync(cancellationToken);


    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) =>
      await _context.Database.RollbackTransactionAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> GetAsync(CancellationToken cancellationToken = default) =>
      await _set.ToListAsync(cancellationToken);

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default) =>
      await _set.CountAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> GetAsync(ISpecificationBuilder<TEntity> specificationBulder, CancellationToken cancellationToken = default) =>
      await SpecificationEvaluator<TEntity>.GetQuery(_set.AsQueryable(), specificationBulder.Build()).ToListAsync(cancellationToken);

    public async Task<TEntity?> GetFirstAsync(ISpecificationBuilder<TEntity> specificationBuilder, CancellationToken cancellationToken = default) =>
      await SpecificationEvaluator<TEntity>.GetQuery(_set.AsQueryable(), specificationBuilder.Build()).FirstOrDefaultAsync(cancellationToken);

    public async Task<int> GetCountAsync(ISpecificationBuilder<TEntity> specificationBuilder, CancellationToken cancellationToken = default) =>
      await ApplySpecification(specificationBuilder.Build()).CountAsync(cancellationToken);
    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
      await _set.FindAsync(id, cancellationToken);

    public async Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
      var entry = _context.Entry(entity);
      if (entry.State == EntityState.Detached)
      {
        var key = _context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();
        var keyValues = key?.Properties.Select(p => p.PropertyInfo.GetValue(entity)).ToArray();

        var existingEntity = await _set.FindAsync(keyValues, cancellationToken);
        if (existingEntity == null)
          await _set.AddAsync(entity, cancellationToken);

        else
          _context.Entry(existingEntity).CurrentValues.SetValues(entity);
      }
      else if (entry.State == EntityState.Added)
        await _set.AddAsync(entity, cancellationToken);
      else
        entry.State = EntityState.Modified;


      //await SaveChangesAsync(); => client should handle it
    }
    public async Task AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
      await _set.AddRangeAsync(entities, cancellationToken);
      //await SaveChangesAsync(); client should handle it
      //await transaction.CommitAsync(); client should handle it

    }

    public Task DeleteAsync(Guid id)
    {
      var entity = _set.Find(id);
      if (entity != null)
        _set.Remove(entity);
      return Task.CompletedTask;
    }

    //private IQueryable<TEntity> ApplySpecificationCount(ISpecification<TEntity> specification)
    //{
    //  // Fetch a queryable that includes all expression-based includes
    //  var queryableResultWithIncludes = specification
    //      .Includes
    //      .Aggregate(_set.AsQueryable(), (current, include) => current.Include(include));

    //  // Modify the queryable to include any string-based include statements
    //  var secondaryResult = specification
    //      .IncludeStrings
    //      .Aggregate(queryableResultWithIncludes, (current, include) => current.Include(include));

    //  // Apply the criteria
    //  return secondaryResult.Where(specification.Criteria);
    //}

    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
      return SpecificationEvaluator<TEntity>.GetQuery(_set.AsQueryable(), specification);
    }
  }
}
