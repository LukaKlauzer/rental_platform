using Application.Interfaces.Data;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Validation;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seeder.Data;
using Seeder.Options;
using Seeder.Services;
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


      var dbContext = services.GetRequiredService<ApplicationDbContext>();
      //await dbContext.Database.MigrateAsync();


      var telemetryRepository = services.GetRequiredService<ITelemetryRepository>();
      var vehicleRepository = services.GetRequiredService<IVehicleRepository>();

      var dataImportService = services.GetRequiredService<IDataImportService>();

      var telemetryValidator = services.GetRequiredService<ITelemetryValidator>();
      var vehicleValidator = services.GetRequiredService<IVehicleValidator>();

      var csvTelemetryDataProvider = services.GetRequiredService<ITelemetryDataProvider>();
      var csvVehicleDataProvider = services.GetRequiredService<IVehicleDataProvider>();
      #endregion DI stuff

      try
      {
        #region Process_Vehicle_Data
        // Process vehicle data
        _logger.LogInformation("Starting vehicle data import");

        var csvVehicleDataResult = await csvVehicleDataProvider.GetVehicleDataAsync();
        if (csvVehicleDataResult.IsSuccess)
        {
          var importResult = await dataImportService.ImportVehiclesAsync(csvVehicleDataResult.Value);

          if (importResult.IsSuccess)
            _logger.LogInformation("Successfully imported {Count} new vehicle records", importResult.Value.Count());
          else
            _logger.LogError("Failed to import vehicle data: {Error}", importResult.Error.Message);
        }
        else
          _logger.LogError("Failed to retrieve vehicle data: {Error}", csvVehicleDataResult.Error.Message);
        #endregion Process_Vehicle_Data

        #region Process_Telemetry_Data
        // Process telemetry data
        _logger.LogInformation("Starting telemetry data import");
        var csvTelemetryDataResult = await csvTelemetryDataProvider.GetTelemetryDataAsync();

        if (csvTelemetryDataResult.IsSuccess)
        {

          var importResult = await dataImportService.ImportTelemetryAsync(csvTelemetryDataResult.Value);

          if (importResult.IsSuccess)
            _logger.LogInformation("Successfully imported {Count} new telemetry records", importResult.Value.Count());
          else
            _logger.LogError("Failed to import telemetry data: {Error}", importResult.Error.Message);
        }
        else
          _logger.LogError("Failed to retrieve telemetry data: {Error}", csvTelemetryDataResult.Error.Message);
        _logger.LogInformation("Data seeding process completed at {Time}", DateTime.UtcNow);
        #endregion Process_Telemetry_Data
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Unexpected error during data seeding.");
      }
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

              services.AddTransient<IDataImportService, DataImportService>();
              services.AddTransient<ITelemetryValidator, TelemetryValidator>();
              services.AddTransient<IVehicleValidator, VehicleValidator>();

              services.AddTransient<ITelemetryDataProvider, CsvTelemetryDataProvider>();
              services.AddTransient<IVehicleDataProvider, CsvVehicleDataProvider>();

            });

  }
}