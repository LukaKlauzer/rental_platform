using Core.Domain.Entities;
using Core.Interfaces.Persistence.GenericRepository;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistance.ConcreteRepositories
{
  public class TelemetryRepository : ITelemetryRepository
  {
    private readonly IRepository<Telemetry> _telemetryRepository;
    private readonly ILogger<TelemetryRepository> _logger;
    public TelemetryRepository(IRepository<Telemetry> telemetryRepository, ILogger<TelemetryRepository> logger)
    {
      _telemetryRepository = telemetryRepository;
      _logger = logger;
    }
    public async Task<Result<Telemetry>> Create(Telemetry telemetry, CancellationToken cancellationToken = default)
    {
      if (telemetry is null)
        return Result<Telemetry>.Failure(Error.NullReferenceError("Telemetry cannot be null"));

      try
      {
        var newTelemetry = await _telemetryRepository.AddOrUpdateAsync(telemetry, cancellationToken);
        _logger.LogInformation("Created telemetry record for VIN {VIN}, Type {Type}", telemetry.VehicleId, telemetry.Name);
        return Result<Telemetry>.Success(newTelemetry);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to create telemetry record for VIN {VIN}, Type {Type}", telemetry.VehicleId, telemetry.Name);
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
        await _telemetryRepository.CommitTransactionAsync(cancellationToken);

        _logger.LogInformation("Successfully created {Count} telemetry records in bulk", telemetries.Count());
        return Result<IEnumerable<Telemetry>>.Success(savedTelemetries);
      }
      catch (Exception ex)
      {
        await _telemetryRepository.RollbackTransactionAsync(cancellationToken);
        _logger.LogError(ex, "Failed to create telemetry records in bulk");
        return Result<IEnumerable<Telemetry>>.Failure(Error.DatabaseWriteError($"Failed to create telemetry records in bulk: {ex.Message}"));
      }
    }
  }
}
