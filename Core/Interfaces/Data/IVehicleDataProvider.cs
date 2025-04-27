using Core.Domain.Entities;
using Core.Result;

namespace Core.Interfaces.Data
{
  public interface IVehicleDataProvider
  {
    Task<Result<IEnumerable<Telemetry>>> GetVehicleDataAsync(CancellationToken cancellationToken = default);
  }
}
