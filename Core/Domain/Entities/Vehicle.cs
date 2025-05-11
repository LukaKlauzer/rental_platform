using Core.Domain.Common;
using Core.Domain.EntityValidation;
using Core.Result;

namespace Core.Domain.Entities
{
  public class Vehicle : Entity
  {
    private Vehicle() { }
    private Vehicle(string vin, string make, string model, int year, float pricePerKmInEuro, float pricePerDayInEuro) 
    {
      Vin = vin;
      Make = make;
      Model = model;
      Year = year;
      PricePerKmInEuro = pricePerKmInEuro;
      PricePerDayInEuro = pricePerDayInEuro;
    }
    public static Result<Vehicle> Create(string vin, string make, string model, int year, float pricePerKmInEuro, float pricePerDayInEuro)
    {
      var validationReult = VehicleValidator.ValidateVehicle(vin, make, model, year, pricePerKmInEuro, pricePerDayInEuro);
      if (validationReult.IsFailure)
        return Result<Vehicle>.Failure(validationReult.Error);

      var vehicle = new Vehicle(vin, make, model, year, pricePerKmInEuro, pricePerDayInEuro);

      return Result<Vehicle>.Success(vehicle);
    }

    

    public string Vin { get; private set; } = string.Empty;
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public float PricePerKmInEuro { get; private set; }
    public float PricePerDayInEuro { get; private set; }

    public ICollection<Rental> Rentals { get; private set; } = new List<Rental>();
    public ICollection<Telemetry> TelemetryRecords { get; private set; } = new List<Telemetry>();
  }
}
