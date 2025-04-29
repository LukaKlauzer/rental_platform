using Core.Domain.Entities;
using Core.Enums;
using Core.Result;

namespace Core.Interfaces.Persistence.SpecificRepository
{
  public interface ITelemetryRepository
  {
    Task<Result<Telemetry>> Create(Telemetry telemetry, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Telemetry>>> CreateBulk(IEnumerable<Telemetry> telemetries, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Telemetry>>> GetMissingRecords(IEnumerable<Telemetry> telemetryToCheck, CancellationToken cancellationToken = default);
    Task<Result<Telemetry>> GetMostRecentBefore(string vin, DateTime dateTime, TelemetryType telemetryType, CancellationToken cancellationToken = default);
    Task<Result<Telemetry>> GetEarliestAfter(string vin, DateTime dateTime, TelemetryType telemetryType, CancellationToken cancellationToken = default);

  }
}
