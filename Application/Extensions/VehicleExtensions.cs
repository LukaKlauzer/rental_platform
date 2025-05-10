using Core.Domain.Entities;
using Application.DTOs.Vehicle;

namespace Application.Extensions
{
  public static class VehicleExtensions
  {
    public static List<VehicleReturnDto> ToListReturnDTO(this List<Vehicle> vehicles)
    {
      if (vehicles is null) return new List<VehicleReturnDto>();

      return vehicles.Select(vehicle => new VehicleReturnDto(
         vehicle.Vin,
         vehicle.Make,
         vehicle.Model,
         vehicle.Year,
         vehicle.PricePerDayInEuro,
         vehicle.PricePerKmInEuro
      )).ToList();
    }

    public static VehicleReturnSingleDto? ToSingleResultDTO(
      this Vehicle vehicle,

      float totalDistanceDriven,
      int totalRentalCount,
      float totalRentalIncome)
    {
      if (vehicle is null) return null;

      return new VehicleReturnSingleDto(

        vehicle.Vin,
        vehicle.Make,
        vehicle.Model,
        vehicle.Year,
        vehicle.PricePerDayInEuro,
        vehicle.PricePerKmInEuro,
        totalDistanceDriven,
        totalRentalCount,
        totalRentalIncome
        );
    }
  }
}
