using Core.Domain.Entities;
using Core.Result;

namespace Core.Interfaces.Persistence.SpecificRepository
{
  public interface ITelemetryRepository
  {
    Task<Result<Telemetry>> Create(Telemetry telemetry, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Telemetry>>> CreateBulk(IEnumerable<Telemetry> telemetries, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<Telemetry>>> GetExistingTelemetry(IEnumerable<Telemetry> telemetryToCheck, CancellationToken cancellationToken = default);
  }
}
