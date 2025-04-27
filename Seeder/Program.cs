using System.Globalization;
using Core.Domain.Entities;
using Core.Enums;
using Core.Interfaces.Data;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Validation;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seeder.Data;
using Seeder.Validators;

namespace RentalPlatform.DataSeeder
{
  class Program
  {


    static async Task Main(string[] args)
    {

    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
              config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
              config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
              config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
              // Register infrastructure services
              services.AddInfrastructureServices(hostContext.Configuration);

              // Register validator services
              services.AddTransient<ITelemetryValidator, TelemetryValidator>();
              services.AddTransient<IVehicleValidator, VehicleValidator>();

              // Register the CSV processor service
              services.AddTransient<ITelemetryDataProvider, CsvTelemetryDataProvider>();
              services.AddTransient<IVehicleDataProvider, CsvVehicleDataProvider>();

            });
  }
    
}