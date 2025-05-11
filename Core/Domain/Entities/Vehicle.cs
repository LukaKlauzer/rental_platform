using System;
using Core.Domain.Common;
using Core.Domain.EntityValidation;
using Core.Enums;
using Core.Result;

namespace Core.Domain.Entities
{
  public class Vehicle : Entity
  {
    public string Vin { get; private set; } = string.Empty;
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public float PricePerKmInEuro { get; private set; }
    public float PricePerDayInEuro { get; private set; }

    public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();
    public IReadOnlyCollection<Telemetry> TelemetryRecords => _telemetryRecords.AsReadOnly();

    private readonly List<Rental> _rentals = new();
    private readonly List<Telemetry> _telemetryRecords = new();

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
    public bool IsAvailableForRental(DateTime startDate, DateTime endDate)
    {
      return !_rentals.Any(r =>
          r.RentalStatus == RentalStatus.Ordered &&
          r.OverlapsWith(startDate, endDate));
    }
    public Result<TelemetryReading> GetTelemetryAtTime(DateTime dateTime, TelemetryType type)
    {
      var relevantReadings = _telemetryRecords
          .Where(t => t.Name == type)
          .OrderBy(t => t.Timestamp)
          .ToList();

      if (!relevantReadings.Any())
        return Result<TelemetryReading>.Failure(
            Error.NotFound($"No {type} telemetry found for vehicle"));

      var reading = relevantReadings
          .LastOrDefault(t => t.GetDateTime() <= dateTime);

      if (reading == null)
        return Result<TelemetryReading>.Failure(
            Error.NotFound($"No {type} telemetry found before {dateTime}"));

      return Result<TelemetryReading>.Success(
          new TelemetryReading(reading.Value, reading.GetDateTime()));
    }
    public Result<TelemetryReading> GetTelemetryAfterTime(DateTime dateTime, TelemetryType type)
    {
      var relevantReadings = _telemetryRecords
          .Where(t => t.Name == type)
          .OrderBy(t => t.Timestamp)
          .ToList();

      if (!relevantReadings.Any())
        return Result<TelemetryReading>.Failure(
            Error.NotFound($"No {type} telemetry found for vehicle"));

      var reading = relevantReadings
          .FirstOrDefault(t => t.GetDateTime() >= dateTime);

      if (reading == null)
        return Result<TelemetryReading>.Failure(
            Error.NotFound($"No {type} telemetry found after {dateTime}"));

      return Result<TelemetryReading>.Success(
          new TelemetryReading(reading.Value, reading.GetDateTime()));
    }
    public Result<VehicleStatistics> CalculateStatistics()
    {
      var completedRentals = _rentals
          .Where(r => r.IsCompleted() && !r.IsCancelled())
          .ToList();

      if (!completedRentals.Any())
        return Result<VehicleStatistics>.Success(
            new VehicleStatistics(0, 0, 0));

      var statistics = completedRentals
          .Select(rental => rental.CalculateCost(this))
          .Where(result => result.IsSuccess)
          .Select(result => result.Value)
          .ToList();

      var totalDistance = statistics.Sum(s => s.Distance);
      var totalIncome = statistics.Sum(s => s.TotalCost);

      return Result<VehicleStatistics>.Success(
          new VehicleStatistics(totalDistance, completedRentals.Count, totalIncome));
    }
  }
  public record VehicleStatistics(
    float TotalDistanceDriven,
    int TotalRentalCount,
    float TotalRentalIncome);

  public record TelemetryReading(float Value, DateTime Timestamp);

}
