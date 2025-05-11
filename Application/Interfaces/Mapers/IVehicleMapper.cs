using Application.DTOs.Vehicle;
using Core.Domain.Entities;
using Core.Result;

namespace Application.Interfaces.Mapers
{
  public interface IVehicleMapper
  {
    public Result<Vehicle> ToEntity(VehicleCreateDto dto);
    public Result<VehicleReturnDto> ToReturnDto(Vehicle entity);
    public Result<VehicleReturnSingleDto> ToReturnSingleDto(Vehicle entity, float totalDistanceDriven = 0f, int totalRentalCount = 0, float totalRentalIncome = 0f);
    public Result<List<VehicleReturnDto>> ToReturnDtoList(IEnumerable<Vehicle> entities);
  }
}
