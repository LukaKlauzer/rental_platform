using Core.DTOs.Vehicle;
using Core.Result;

namespace Core.Interfaces.Services
{
  public interface IVeachelService
  {
    public Task<Result<List<VehicleReturnDTO>>> GetAll();
    public Task<Result<VehicleReturnSingleDTO>> GetByVin(string vin);
  }
}