using Core.Domain.Entities;
using Core.Result;

namespace Core.Interfaces.Data
{
  public interface ITelemetryDataProvider
  {
    Task<Result<IEnumerable<Telemetry>>> GetTelemetryDataAsync(CancellationToken cancellationToken = default);
  }
}
