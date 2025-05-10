using Application.Services;
using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
  public static class DependencyInjection
  {

    public static IServiceCollection AddApplicationServices(this IServiceCollection services) 
    {
      services.AddScoped<ICustomerService, CustomerSevice>();
      services.AddScoped<IRentalService, RentalService>();
      services.AddScoped<IVeachelService, VehicleService>();

      return services;
    }

  }
}
