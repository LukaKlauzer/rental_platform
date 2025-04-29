using Core.Domain.Entities;
using Core.Interfaces.Validation;
using Microsoft.Extensions.Logging;

namespace Seeder.Validators
{
  internal class VehicleValidator : IVehicleValidator
  {
    private readonly ILogger<VehicleValidator> _logger;
    public VehicleValidator(ILogger<VehicleValidator> logger) 
    {
      _logger = logger;
    }
    public IEnumerable<Vehicle> FilterValidVehicles(IEnumerable<Vehicle> vehicles) =>
      vehicles.Where(IsValid).ToList();

    public bool IsValid(Vehicle vehicle)
    {
      // Check if VIN is missing
      if (string.IsNullOrWhiteSpace(vehicle.Vin))
      {
        _logger.LogWarning("Vehicle record skipped: Missing VIN");
        return false;
      }

      // Check if make is missing
      if (string.IsNullOrWhiteSpace(vehicle.Make))
      {
        _logger.LogWarning("Vehicle record skipped for VIN {VIN}: Missing make", vehicle.Vin);
        return false;
      }

      // Check if model is missing
      if (string.IsNullOrWhiteSpace(vehicle.Model))
      {
        _logger.LogWarning("Vehicle record skipped for VIN {VIN}: Missing model", vehicle.Vin);
        return false;
      }

      // Check if year is valid (must be in a reasonable range)
      if (vehicle.Year > DateOnly.FromDateTime(DateTime.Now).Year)
      {
        _logger.LogWarning("Vehicle record skipped for VIN {VIN}: Invalid year {Year}", vehicle.Vin, vehicle.Year);
        return false;
      }

      // Check if pricePerKmInEuro is within a valid range (example: positive)
      if (vehicle.PricePerKmInEuro <= 0)
      {
        _logger.LogWarning("Vehicle record skipped for VIN {VIN}: Invalid price per km {PricePerKmInEuro}",
            vehicle.Vin, vehicle.PricePerKmInEuro);
        return false;
      }

      // Check if pricePerDayInEuro is within a valid range (example: positive)
      if (vehicle.PricePerDayInEuro <= 0)
      {
        _logger.LogWarning("Vehicle record skipped for VIN {VIN}: Invalid price per day {PricePerDayInEuro}",
            vehicle.Vin, vehicle.PricePerDayInEuro);
        return false;
      }

      return true;
    }
  }
}
