using Core.Domain.Entities;
using Core.Result;

namespace Core.Interfaces.Data
{
  public interface IVehicleDataProvider
  {
    public Task<Result<IEnumerable<Vehicle>>> GetVehicleDataAsync(CancellationToken cancellationToken = default);
  }
}
