using Application.DTOs.Vehicle;
using Application.Interfaces.Mapers;
using Core.Domain.Entities;
using Core.Result;

namespace Application.Mapers
{
  internal class VehicleMapper : IVehicleMapper
  {
    public Result<Vehicle> ToEntity(VehicleCreateDto dto)
    {
      if (dto is null)
        return Result<Vehicle>.Failure(Error.NullReferenceError("VehicleCreateDTO cannot be null"));

      return Vehicle.Create(
        vin: dto.Vin,
        make: dto.Make,
        model: dto.Model,
        year: dto.Year.Year,
        pricePerKmInEuro: dto.PricePerKmInEuro,
        pricePerDayInEuro: dto.PricePerDayInEuro
      );
    }

    public Result<VehicleReturnDto> ToReturnDto(Vehicle entity)
    {
      if (entity is null)
        return Result<VehicleReturnDto>.Failure(Error.NullReferenceError("Vehicle entity cannot be null"));

      var dto = new VehicleReturnDto(
        Vin: entity.Vin,
        Make: entity.Make,
        Model: entity.Model,
        Year: entity.Year,
        PricePerKmInEuro: entity.PricePerKmInEuro,
        PricePerDayInEuro: entity.PricePerDayInEuro
      );

      return Result<VehicleReturnDto>.Success(dto);
    }

    public Result<VehicleReturnSingleDto> ToReturnSingleDto(
      Vehicle entity,
      float totalDistanceDriven = 0f,
      int totalRentalCount = 0,
      float totalRentalIncome = 0f)
    {
      if (entity is null)
        return Result<VehicleReturnSingleDto>.Failure(Error.NullReferenceError("Vehicle entity cannot be null"));

      var dto = new VehicleReturnSingleDto(
        Vin: entity.Vin,
        Make: entity.Make,
        Model: entity.Model,
        Year: entity.Year,
        PricePerKmInEuro: entity.PricePerKmInEuro,
        PricePerDayInEuro: entity.PricePerDayInEuro,
        TotalDistanceDriven: totalDistanceDriven,
        TotalRentalCount: totalRentalCount,
        TotalRentalIncome: totalRentalIncome
      );

      return Result<VehicleReturnSingleDto>.Success(dto);
    }

    public Result<List<VehicleReturnDto>> ToReturnDtoList(IEnumerable<Vehicle> entities)
    {
      if (entities is null)
        return Result<List<VehicleReturnDto>>.Failure(Error.NullReferenceError("Vehicle collection cannot be null"));

      var dtos = new List<VehicleReturnDto>();

      foreach (var entity in entities)
      {
        var dtoResult = ToReturnDto(entity);
        if (dtoResult.IsFailure)
          return Result<List<VehicleReturnDto>>.Failure(dtoResult.Error);

        dtos.Add(dtoResult.Value);
      }

      return Result<List<VehicleReturnDto>>.Success(dtos);
    }
  }
}
