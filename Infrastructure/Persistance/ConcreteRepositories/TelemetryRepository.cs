using Core.Domain.Entities;
using Core.Enums;
using Application.Interfaces.Persistence.GenericRepository;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Infrastructure.Persistance.GenericRepository;

namespace Infrastructure.Persistance.ConcreteRepositories
{
  public class TelemetryRepository : ITelemetryRepository
  {
    private readonly IRepository<Telemetry> _telemetryRepository;

    public TelemetryRepository(
      IRepository<Telemetry> telemetryRepository
      )
    {
      _telemetryRepository = telemetryRepository;
    }

    public async Task<Result<Telemetry>> Create(Telemetry telemetry, CancellationToken cancellationToken = default)
    {
      if (telemetry is null)
        return Result<Telemetry>.Failure(Error.NullReferenceError("Telemetry cannot be null"));

      try
      {
        var newTelemetry = await _telemetryRepository.AddOrUpdateAsync(telemetry, cancellationToken);
        //_logger.LogInformation("Created telemetry record for VIN {VIN}, Type {Type}", telemetry.VehicleId, telemetry.Name);
        return Result<Telemetry>.Success(newTelemetry);
      }
      catch (Exception ex)
      {
        //_logger.LogError(ex, "Failed to create telemetry record for VIN {VIN}, Type {Type}", telemetry.VehicleId, telemetry.Name);
        return Result<Telemetry>.Failure(Error.DatabaseWriteError($"Failed to create telemetry: {ex.Message}"));
      }
    }

    public async Task<Result<IEnumerable<Telemetry>>> CreateBulk(IEnumerable<Telemetry> telemetries, CancellationToken cancellationToken = default)
    {
      if (telemetries is null || !telemetries.Any())
        return Result<IEnumerable<Telemetry>>.Success(Enumerable.Empty<Telemetry>());

      try
      {
        await _telemetryRepository.BeginTransactionAsync(cancellationToken);
        var savedTelemetries = await _telemetryRepository.AddAsync(telemetries, cancellationToken);
        await _telemetryRepository.SaveChangesAsync();
        await _telemetryRepository.CommitTransactionAsync(cancellationToken);


        return Result<IEnumerable<Telemetry>>.Success(savedTelemetries);
      }
      catch (Exception ex)
      {
        await _telemetryRepository.RollbackTransactionAsync(cancellationToken);

        return Result<IEnumerable<Telemetry>>.Failure(Error.DatabaseWriteError($"Failed to create telemetry records in bulk: {ex.Message}"));
      }
    }
    public async Task<Result<IEnumerable<Telemetry>>> GetMissingRecords(IEnumerable<Telemetry> recordsToCheck,CancellationToken cancellationToken = default)
    {
      try
      {
        var telemetryToCheck = recordsToCheck.ToList();

        // Group by VehicleId to make fewer queries
        var recordsByVehicle = telemetryToCheck
            .GroupBy(t => t.VehicleId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Create a set to track existing keys for fast lookup
        var existingKeys = new HashSet<(string, TelemetryType, int)>();

        // Query once per vehicle
        foreach (var vehicleId in recordsByVehicle.Keys)
        {
          // Query for all existing records for this vehicle
          var spec = new SpecificationBuilder<Telemetry>(t => t.VehicleId == vehicleId);
          var existingRecords = await _telemetryRepository.GetAsync(spec, cancellationToken);

          // Add the existing keys to our set
          foreach (var record in existingRecords)
            existingKeys.Add((record.VehicleId, record.Name, record.Timestamp));
        }

        // Find all records that don't exist in the database
        var missingRecords = telemetryToCheck
            .Where(record => !existingKeys.Contains((record.VehicleId, record.Name, record.Timestamp)))
            .ToList();


        return Result<IEnumerable<Telemetry>>.Success(missingRecords);
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Telemetry>>.Failure(
            Error.DatabaseReadError($"Error checking for existing telemetry: {ex.Message}"));
      }
    }
    public async Task<Result<Telemetry>> GetMostRecentBefore(string vin, DateTime dateTime, TelemetryType telemetryType, CancellationToken cancellationToken = default)
    {
      var timestamp = DateTimeToUnixSeconds(dateTime);
      try
      {
        var specification = new SpecificationBuilder<Telemetry>(telemetry =>
        telemetry.Timestamp <= timestamp &&
        telemetry.VehicleId == vin &&
        telemetry.Name == telemetryType);

        var records = await _telemetryRepository.GetAsync(specification, cancellationToken);

        var mostRecentRecord = records
            .OrderByDescending(t => t.Timestamp)
            .FirstOrDefault();

        if (mostRecentRecord is not null)
          return Result<Telemetry>.Success(mostRecentRecord);
        return Result<Telemetry>.Failure(Error.NotFound($"Teletry for: '{dateTime}' not found"));
      }
      catch (Exception ex)
      {
        return Result<Telemetry>.Failure(Error.DatabaseReadError($"Unable to retrieve telemetry: {ex.Message}"));
      }
    }
    public async Task<Result<Telemetry>> GetEarliestAfter(string vin, DateTime dateTime, TelemetryType telemetryType, CancellationToken cancellationToken = default)
    {
      var timestamp = DateTimeToUnixSeconds(dateTime);
      try
      {
        var specification = new SpecificationBuilder<Telemetry>(telemetry =>
        telemetry.Timestamp >= timestamp &&
        telemetry.VehicleId == vin &&
        telemetry.Name == telemetryType); ;

        var records = await _telemetryRepository.GetAsync(specification, cancellationToken);
        var earliestRecord = records
            .OrderBy(t => t.Timestamp)  // Note: OrderBy instead of OrderByDescending
            .FirstOrDefault();

        if (earliestRecord is not null)
          return Result<Telemetry>.Success(earliestRecord);
        return Result<Telemetry>.Failure(Error.NotFound($"Teletry for: '{dateTime}' not found"));
      }
      catch (Exception ex)
      {
        return Result<Telemetry>.Failure(Error.DatabaseReadError($"Unable to retrieve telemetry: {ex.Message}"));
      }
    }
    public long DateTimeToUnixSeconds(DateTime time) => ((DateTimeOffset)time).ToUnixTimeSeconds();
    public DateTime UnixSecondsToDateTime(long timestamp, bool local = false)
    {
      var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
      return local ? offset.LocalDateTime : offset.UtcDateTime;
    }
  }
}
