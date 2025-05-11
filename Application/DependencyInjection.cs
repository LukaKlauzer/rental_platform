using Application.Services;
using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Mapers;
using Application.Mapers;

namespace Application
{
  public static class DependencyInjection
  {

    public static IServiceCollection AddApplicationServices(this IServiceCollection services) 
    {
      // Services
      services.AddScoped<ICustomerService, CustomerService>();
      services.AddScoped<IRentalService, RentalService>();
      services.AddScoped<IVehicleService, VehicleService>();

      // Mappers
      services.AddScoped<ICustomerMapper, CustomerMapper>();
      services.AddScoped<IVehicleMapper, VehicleMapper>();
      services.AddScoped<IRentalMapper, RentalMapper>();

      return services;
    }

  }
}
