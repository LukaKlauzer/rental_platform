using Core.Domain.Common;
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
      var validationReult = ValidateVehicle(vin, make, model, year, pricePerKmInEuro, pricePerDayInEuro);
      if (validationReult.IsFailure)
        return Result<Vehicle>.Failure(validationReult.Error);

      var vehicle = new Vehicle(vin, make, model, year, pricePerKmInEuro, pricePerDayInEuro);

      return Result<Vehicle>.Success(vehicle);
    }

    private static Result<bool> ValidateVehicle(string vin, string make, string model, int year, float pricePerKmInEuro, float pricePerDayInEuro)
    {
      if (string.IsNullOrEmpty(vin))
        return Result<bool>.Failure(Error.ValidationError("Vin can not be null or empty string"));
      // TODO more VIN validations... or create value object... and do validation in there

      if (string.IsNullOrEmpty(make))
        return Result<bool>.Failure(Error.ValidationError("Make can not be null or empty string"));

      if (string.IsNullOrEmpty(model))
        return Result<bool>.Failure(Error.ValidationError("Model can not be null or empty string"));

      if (year < 0)
        return Result<bool>.Failure(Error.ValidationError("Year can not be negativee number"));

      if (pricePerKmInEuro < 0)
        return Result<bool>.Failure(Error.ValidationError("Price per km in euro can not be negativee number"));

      if (pricePerDayInEuro < 0)
        return Result<bool>.Failure(Error.ValidationError("Price per day in euro can not be negativee number"));

      return Result<bool>.Success(true);
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
