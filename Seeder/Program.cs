using Core.Interfaces.Data;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Validation;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seeder.Data;
using Seeder.Options;
using Seeder.Validators;

namespace RentalPlatform.DataSeeder
{
  class Program
  {
    private static ILogger<Program> _logger;

    static async Task Main(string[] args)
    {
      #region DI stuff
      var host = CreateHostBuilder(args).Build();

      using var scope = host.Services.CreateScope();
      var services = scope.ServiceProvider;

      _logger = services.GetRequiredService<ILogger<Program>>();
      _logger.LogInformation("Data seeding process started at {Time}", DateTime.UtcNow);

      var telemetryRepository = services.GetRequiredService<ITelemetryRepository>();
      var vehicleRepository = services.GetRequiredService<IVehicleRepository>();

      var telemetryValidator = services.GetRequiredService<ITelemetryValidator>();
      var vehicleValidator = services.GetRequiredService<IVehicleValidator>();

      var csvTelemetryDataProvider = services.GetRequiredService<ITelemetryDataProvider>();
      var csvVehicleDataProvider = services.GetRequiredService<IVehicleDataProvider>();
      #endregion DI stuff

      // Process vehicle data
      _logger.LogInformation("Starting vehicle data import");
      var csvVehicleDataResult = await csvVehicleDataProvider.GetVehicleDataAsync();

      if (csvVehicleDataResult.IsSuccess)
      {
        var vehicleCreateResult = await vehicleRepository.CreateBulk(csvVehicleDataResult.Value.ToList());

        if (vehicleCreateResult.IsSuccess)
          _logger.LogInformation("Successfully saved {Count} vehicle records", vehicleCreateResult.Value.Count());
        else
          _logger.LogError("Failed to save vehicle data: {Error}", vehicleCreateResult.Error.Message);
      }
      else
        _logger.LogError("Failed to retrieve vehicle data: {Error}", csvVehicleDataResult.Error.Message);

      // Process telemetry data
      _logger.LogInformation("Starting telemetry data import");
      var csvTelemetryDataResult = await csvTelemetryDataProvider.GetTelemetryDataAsync();

      if (csvTelemetryDataResult.IsSuccess)
      {
        var telemetryCreateResult = await telemetryRepository.CreateBulk(csvTelemetryDataResult.Value);

        if (telemetryCreateResult.IsSuccess)
          _logger.LogInformation("Successfully saved {Count} telemetry records", telemetryCreateResult.Value.Count());
        else
          _logger.LogError("Failed to save telemetry data: {Error}", telemetryCreateResult.Error.Message);
      }
      else
        _logger.LogError("Failed to retrieve telemetry data: {Error}", csvTelemetryDataResult.Error.Message);

      _logger.LogInformation("Data seeding process completed at {Time}", DateTime.UtcNow);
    }
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, logging) =>
            {
              logging.ClearProviders();
              logging.AddConsole();
              logging.AddDebug();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
              config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
              config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
              config.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
              services.AddInfrastructureServices(hostContext.Configuration);

              services.Configure<CsvDataOptions>(hostContext.Configuration.GetSection("CsvFiles"));

              services.AddTransient<ITelemetryValidator, TelemetryValidator>();
              services.AddTransient<IVehicleValidator, VehicleValidator>();

              services.AddTransient<ITelemetryDataProvider, CsvTelemetryDataProvider>();
              services.AddTransient<IVehicleDataProvider, CsvVehicleDataProvider>();

            });
  }
}