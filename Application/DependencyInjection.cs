using Application.Services;
using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Mapers;
using Application.Mapers;
using Application.Interfaces.DataValidation;
using Application.DTOs.Rental;
using Application.DTOs.Customer;
using Application.DTOs.Vehicle;

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

      // Validators
      services.AddScoped<ICustomerValidator, CustomerDtoValidator>();
      services.AddScoped<IRentalValidator, RentalDtoValidator>();
      services.AddScoped<IVehicleValidator, VehicleDtoValidator>();

      return services;
    }

  }
}
