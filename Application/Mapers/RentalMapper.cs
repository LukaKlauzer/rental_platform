using Application.DTOs.Rental;
using Application.Interfaces.Mapers;
using Core.Domain.Entities;
using Core.Result;

namespace Application.Mapers
{
  internal class RentalMapper : IRentalMapper
  {
    public Result<RentalReturnDto> ToReturnDto(Rental entity)
    {
      if (entity is null)
        return Result<RentalReturnDto>.Failure(Error.NullReferenceError("Rental entity cannot be null"));

      var dto = new RentalReturnDto(
        Id: entity.ID,
        StartDate: entity.StartDate,
        EndDate: entity.EndDate,
        RentalStatus: entity.RentalStatus,
        VehicleId: entity.VehicleId,
        CustomerId: entity.CustomerId
      );

      return Result<RentalReturnDto>.Success(dto);
    }

    public Result<RentalReturnSingleDto> ToReturnSingleDto(
      Rental entity,
      float? distanceTraveled)
    {
      if (entity is null)
        return Result<RentalReturnSingleDto>.Failure(Error.NullReferenceError("Rental entity cannot be null for single DTO mapping"));


      var dto = new RentalReturnSingleDto(
        Id: entity.ID,
        StartDate: entity.StartDate,
        EndDate: entity.EndDate,
        RentalStatus: entity.RentalStatus,
        VehicleId: entity.VehicleId,
        CustomerId: entity.CustomerId,
        DistanceTraveled: distanceTraveled,
        BatterySOCSAtStart: entity.BatterySOCStart,
        BatterySOCAtEnd: entity.BatterySOCEnd ?? 0f
      );

      return Result<RentalReturnSingleDto>.Success(dto);
    }

    public Result<List<RentalReturnDto>> ToReturnDtoList(IEnumerable<Rental> entities)
    {
      if (entities is null)
        return Result<List<RentalReturnDto>>.Failure(Error.NullReferenceError("Rental collection can not be null"));

      var dtos = new List<RentalReturnDto>();

      foreach (var entity in entities)
      {
        var dtoResult = ToReturnDto(entity);
        if (dtoResult.IsFailure)
          return Result<List<RentalReturnDto>>.Failure(dtoResult.Error);

        dtos.Add(dtoResult.Value);
      }

      return Result<List<RentalReturnDto>>.Success(dtos);
    }

    public Result<Rental> ToEntity(
      RentalCreateDto dto,
      float odometorStart,
      float batterySOCStart,
      float? odometorEnd = null,
      float? batterySOCEnd = null)
    {
      return Rental.Create(
        startDate: dto.StartDate,
        endDate: dto.EndDate,
        odometerStart: odometorStart,
        batterySOCStart: batterySOCStart,
        vehicleId: dto.VehicleId,
        customerId: dto.CustomerId,
        odometerEnd: odometorEnd,
        batterySOCEnd: batterySOCEnd);
    }
  }
}
