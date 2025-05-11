using Application.Interfaces.Persistence.GenericRepository;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Domain.Entities;
using Infrastructure.Persistance.ConcreteRepositories;
using Infrastructure.Persistance.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure
{
  public static class DependencyInjection
  {
    public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      IConfiguration config)
    {
      // DbContext setup
      services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
          config.GetConnectionString("DefaultConnection"),
          b => b.MigrationsAssembly("Infrastructure")
          ));


      services.AddScoped<IRepository<Telemetry>, Repository<Telemetry>>();
      services.AddScoped<IRepository<Customer>, Repository<Customer>>();
      services.AddScoped<IRepository<Vehicle>, Repository<Vehicle>>();
      services.AddScoped<IRepository<Rental>, Repository<Rental>>();

      services.AddScoped<IVehicleRepository, VehicleRepository>();
      services.AddScoped<ITelemetryRepository, TelemetryRepository>();
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      services.AddScoped<IRentalRepository, RentalRepository>();


      return services;
    }

  }
}
