using Application.DTOs.Vehicle;
using Core.Result;

namespace Application.Interfaces.Services
{
  public interface IVeachelService
  {
    public Task<Result<List<VehicleReturnDto>>> GetAll();
    public Task<Result<VehicleReturnSingleDto>> GetByVin(string vin);
  }
}