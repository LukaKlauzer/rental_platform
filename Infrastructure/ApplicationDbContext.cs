using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
  internal class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
  {
    public ApplicationDbContext(DbContextOptions<DbContext> options):
      base(options) 
    {
      
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    // Override SaveChanges to automatically set DateCreated and DateModified
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      var entries = ChangeTracker
          .Entries()
          .Where(e => e.Entity is Core.Domain.Common.Entity &&
                     (e.State == EntityState.Added || e.State == EntityState.Modified));

      foreach (var entityEntry in entries)
      {
        if (entityEntry.Entity is Core.Domain.Common.Entity entity)
        {
          var now = DateTime.UtcNow;

          if (entityEntry.State == EntityState.Added)
            entity.DateCreated = now;
          

          entity.DateModified = now;
        }
      }

      return base.SaveChangesAsync(cancellationToken);
    }
  }
}
