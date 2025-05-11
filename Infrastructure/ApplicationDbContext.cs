using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
      base(options)
    {

    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<Telemetry> Telemetries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Customer>(
        entity => 
        {
          entity.HasKey(e=>e.ID);
          entity.Property(e=>e.Name).IsRequired();
        });

      modelBuilder.Entity<Vehicle>(entity => 
      {
        entity.HasKey(e => e.Vin);
        entity.Property(e => e.Vin).IsRequired();
        entity.Property(e => e.Make).IsRequired();
        entity.Property(e => e.Model).IsRequired();
        entity.Property(e => e.PricePerKmInEuro).IsRequired();
        entity.Property(e => e.PricePerDayInEuro).IsRequired();
      });

      modelBuilder.Entity<Rental>(entity =>
      {
        entity.HasKey(e=>e.ID);

        entity.Property(e => e.StartDate).IsRequired();
        entity.Property(e => e.OdometerStart).IsRequired();
        entity.Property(e => e.BatterySOCStart).IsRequired();

        entity.HasOne(r => r.Customer)
              .WithMany(r => r.Rentals)
              .HasForeignKey(r=>r.CustomerId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(r => r.Vehicle)
              .WithMany(r => r.Rentals)
              .HasForeignKey(r=>r.VehicleId)
              .OnDelete(DeleteBehavior.Restrict);
      });

      modelBuilder.Entity<Telemetry>(entity =>
      {
        entity.HasKey(e => e.ID);

        entity.HasOne(t => t.Vehicle)
              .WithMany(v => v.TelemetryRecords)
              .HasForeignKey(t => t.VehicleId)
              .OnDelete(DeleteBehavior.Cascade);
        
        entity.HasIndex(t => new { t.VehicleId, t.Name, t.Timestamp })
              .IsUnique()
              .HasDatabaseName("Telemetry_VehicleId_Name_Timestamp");
      });
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
