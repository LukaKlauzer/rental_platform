using Application.Interfaces.Persistence.SpecificRepository;
using Core.Domain.Entities;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Seeder.Services
{
  public interface IDataImportService
  {
    Task<Result<IEnumerable<Vehicle>>> ImportVehiclesAsync(IEnumerable<Vehicle> vehicles, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Telemetry>>> ImportTelemetryAsync(IEnumerable<Telemetry> telemetries, CancellationToken cancellationToken = default);
  }
  internal class DataImportService : IDataImportService
  {
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(
      IVehicleRepository vehicleRepository,
      ITelemetryRepository telemetryRepository,
      ILogger<DataImportService> logger)
    {
      _vehicleRepository = vehicleRepository;
      _telemetryRepository = telemetryRepository;
      _logger = logger;
    }
    public async Task<Result<IEnumerable<Telemetry>>> ImportTelemetryAsync(IEnumerable<Telemetry> telemetries, CancellationToken cancellationToken = default)
    {
      try
      {
        var telemetryList = telemetries.ToList();
        _logger.LogInformation($"Starting telemetry import. Total records: {telemetryList.Count}");
        var missingRecordsResult = await _telemetryRepository.GetMissingRecords(telemetryList, cancellationToken);

        if (missingRecordsResult.IsFailure)
          return Result<IEnumerable<Telemetry>>.Failure(missingRecordsResult.Error);

        var newTelemetries = missingRecordsResult.Value.ToList();

        if (!newTelemetries.Any())
        {
          _logger.LogInformation("No new telemetry records to import");
          return Result<IEnumerable<Telemetry>>.Success(Enumerable.Empty<Telemetry>());
        }

        _logger.LogInformation($"Found {telemetryList.Count - newTelemetries.Count} existing records, {newTelemetries.Count} new records to import");

        // Import new telemetry records
        var importResult = await _telemetryRepository.CreateBulk(newTelemetries, cancellationToken);

        if (importResult.IsFailure)
          return Result<IEnumerable<Telemetry>>.Failure(importResult.Error);

        _logger.LogInformation($"Successfully imported {importResult.Value.Count()} telemetry records");
        return Result<IEnumerable<Telemetry>>.Success(importResult.Value);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error importing telemetry");
        return Result<IEnumerable<Telemetry>>.Failure(
            Error.DatabaseWriteError($"Failed to import telemetry: {ex.Message}"));
      }
    }

    public async Task<Result<IEnumerable<Vehicle>>> ImportVehiclesAsync(IEnumerable<Vehicle> vehicles, CancellationToken cancellationToken = default)
    {
      try
      {
        var vehicleList = vehicles.ToList();
        _logger.LogInformation($"Starting vehicle import. Total vehicles: {vehicleList.Count}");

        // Check for existing vehicles
        var newVins = vehicleList.Select(v => v.Vin).ToList();
        var existingVehiclesResult = await _vehicleRepository.GetByVins(newVins, cancellationToken);

        if (existingVehiclesResult.IsFailure)
          return Result<IEnumerable<Vehicle>>.Failure(existingVehiclesResult.Error);

        var existingVins = existingVehiclesResult.Value.Select(v => v.Vin).ToHashSet();
        var newVehicles = vehicleList.Where(v => !existingVins.Contains(v.Vin)).ToList();

        if (!newVehicles.Any())
        {
          _logger.LogInformation("No new vehicles to import");
          return Result<IEnumerable<Vehicle>>.Success(Enumerable.Empty<Vehicle>());
        }

        _logger.LogInformation($"Found {existingVins.Count} existing vehicles, {newVehicles.Count} new vehicles to import");

        // Import new vehicles
        var importResult = await _vehicleRepository.CreateBulk(newVehicles, cancellationToken);

        if (importResult.IsFailure)
          return Result<IEnumerable<Vehicle>>.Failure(importResult.Error);

        _logger.LogInformation($"Successfully imported {importResult.Value.Count} vehicles");
        return Result<IEnumerable<Vehicle>>.Success(importResult.Value);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error importing vehicles");
        return Result<IEnumerable<Vehicle>>.Failure(
            Error.DatabaseWriteError($"Failed to import vehicles: {ex.Message}"));
      }
    }
  }
}
