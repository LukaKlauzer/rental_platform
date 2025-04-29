using Core.Domain.Entities;
using Core.Result;

namespace Core.Interfaces.Persistence.SpecificRepository
{
  public interface IVehicleRepository
  {
    public Task<Result<Vehicle>> Create(Vehicle vehicle, CancellationToken cancellationToken = default);
    Task<Result<List<Vehicle>>> CreateBulk(List<Vehicle> vehicles, CancellationToken cancellationToken = default);
    public Task<Result<List<Vehicle>>> GetAll(CancellationToken cancellationToken = default);
    public Task<Result<Vehicle>> GetById(string vin, CancellationToken cancellationToken = default);
    public Task<Result<Vehicle>> GetByVin(string vin, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<Vehicle>>> GetByVins(List<string> vins, CancellationToken cancellationToken = default);
  }
}