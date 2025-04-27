using Core.Domain.Entities;
using Core.DTOs.Vehicle;

namespace Core.Extensions
{
  public static class VehicleExtensions
  {
    public static List<VehicleReturnDTO> ToListReturnDTO(this List<Vehicle> vehicles)
    {
      if (vehicles is null) return new List<VehicleReturnDTO>();

      return vehicles.Select(vehicle => new VehicleReturnDTO()
      {
        Vin = vehicle.Vin,
        Make = vehicle.Make,
        Model = vehicle.Model,
        Year = vehicle.Year,
        PricePerDayInEuro = vehicle.PricePerDayInEuro,
        PricePerKmInEuro = vehicle.PricePerKmInEuro,
      }).ToList();
    }

    public static VehicleReturnSingleDTO? ToSingleResultDTO(this Vehicle vehicle)
    {
      if (vehicle is null) return null;

      return new VehicleReturnSingleDTO()
      {
        Vin = vehicle.Vin,
        Make = vehicle.Make,
        Model = vehicle.Model,
        Year = vehicle.Year,
        PricePerDayInEuro = vehicle.PricePerDayInEuro,
        PricePerKmInEuro = vehicle.PricePerKmInEuro,

      };
    }
  }
}
