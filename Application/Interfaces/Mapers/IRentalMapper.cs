using Application.DTOs.Rental;
using Core.Domain.Entities;
using Core.Result;

namespace Application.Interfaces.Mapers
{
  public interface IRentalMapper
  {
    Result<Rental> ToEntity(
      RentalCreateDto dto,
      float odometorStart,
      float batterySOCStart,
      float? odometorEnd = null,
      float? batterySOCEnd = null);
    Result<RentalReturnDto> ToReturnDto(Rental entity);
    Result<RentalReturnSingleDto> ToReturnSingleDto(
      Rental entity,
      float? distanceTraveled = null);
    Result<List<RentalReturnDto>> ToReturnDtoList(IEnumerable<Rental> entities);
  }
}
