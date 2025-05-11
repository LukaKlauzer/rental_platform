using Core.Domain.Common;
using Core.Domain.EntityValidation;
using Core.Enums;
using Core.Result;

namespace Core.Domain.Entities
{
  public class Rental : EntityID
  {
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public RentalStatus RentalStatus { get; private set; }
    public float OdometerStart { get; private set; }
    public float? OdometerEnd { get; private set; }
    public float BatterySOCStart { get; private set; }
    public float? BatterySOCEnd { get; private set; }

    public string VehicleId { get; private set; } = string.Empty;
    public int CustomerId { get; private set; }

    public Vehicle Vehicle { get; private set; } = null!;
    public Customer Customer { get; private set; } = null!;
    private Rental() { }
    private Rental(
      DateTime startDate,
      DateTime endDate,
      float odometerStart,
      float batterySOCStart,
      string vehicleId,
      int customerId,
      float? odometerEnd = null,
      float? batterySOCEnd = null)
    {
      StartDate = startDate;
      EndDate = endDate;
      OdometerStart = odometerStart;
      BatterySOCStart = batterySOCStart;

      if (odometerEnd is not null)
        OdometerEnd = odometerEnd;

      if (batterySOCEnd is not null)
        BatterySOCEnd = batterySOCEnd;

      VehicleId = vehicleId;
      CustomerId = customerId;

      RentalStatus = RentalStatus.Ordered;
    }

    public static Result<Rental> Create(
      DateTime startDate,
      DateTime endDate,
      float odometerStart,
      float batterySOCStart,
      string vehicleId,
      int customerId,
      float? odometerEnd = null,
      float? batterySOCEnd = null)
    {
      var validateResult = RentalValidator.ValidateData(startDate, endDate, odometerStart, batterySOCStart, vehicleId, customerId, odometerEnd, batterySOCEnd);
      if (validateResult.IsFailure)
        return Result<Rental>.Failure(validateResult.Error);

      var rental = new Rental(startDate, endDate, odometerStart, batterySOCStart, vehicleId, customerId, odometerEnd, batterySOCEnd);

      return Result<Rental>.Success(rental);
    }

    public Result<bool> Cancel()
    {
      if (RentalStatus is not RentalStatus.Ordered)
        return Result<bool>.Failure(Error.ValidationError("Only ordered rentals can be canceled"));

      RentalStatus = RentalStatus.Cancelled;

      return Result<bool>.Success(true);

    }
    public Result<bool> CanUpdateDates(
        DateTime newStartDate,
        DateTime newEndDate,
        Customer customer,
        Vehicle vehicle)
    {
      if (RentalStatus != RentalStatus.Ordered)
        return Result<bool>.Failure(
            Error.ValidationError("Cannot update dates for non-active rental"));

      // Check if customer has other overlapping rentals (excluding this one)
      var customerHasOtherOverlap = customer.Rentals
          .Where(r => r.ID != this.ID && r.RentalStatus == RentalStatus.Ordered)
          .Any(r => r.OverlapsWith(newStartDate, newEndDate));

      if (customerHasOtherOverlap)
        return Result<bool>.Failure(
            Error.ValidationError("Customer has another rental in this time period"));

      // Check if vehicle has other overlapping rentals (excluding this one)
      var vehicleHasOtherOverlap = vehicle.Rentals
          .Where(r => r.ID != this.ID && r.RentalStatus == RentalStatus.Ordered)
          .Any(r => r.OverlapsWith(newStartDate, newEndDate));

      if (vehicleHasOtherOverlap)
        return Result<bool>.Failure(
            Error.ValidationError("Vehicle is not available for the new dates"));

      return Result<bool>.Success(true);
    }
    public Result<bool> UpdateDates(
        DateTime? newStartDate,
        DateTime? newEndDate,
        Customer customer,
        Vehicle vehicle)
    {
      if (!newStartDate.HasValue && !newEndDate.HasValue)
        return Result<bool>.Failure(
            Error.ValidationError("At least one date must be provided for update"));

      var effectiveStartDate = newStartDate ?? StartDate;
      var effectiveEndDate = newEndDate ?? EndDate;

      var validationResult = RentalValidator.ValidateDates(effectiveStartDate, effectiveEndDate);
      if (validationResult.IsFailure)
        return validationResult;

      // Check if update is allowed
      var canUpdateResult = CanUpdateDates(effectiveStartDate, effectiveEndDate, customer, vehicle);
      if (canUpdateResult.IsFailure)
        return canUpdateResult;

      StartDate = effectiveStartDate;
      EndDate = effectiveEndDate;

      return Result<bool>.Success(true);
    }
    public bool IsCompleted()
    {
      return OdometerEnd.HasValue &&
             BatterySOCEnd.HasValue &&
             RentalStatus == RentalStatus.Ordered;
    }
    public bool IsCancelled() => RentalStatus == RentalStatus.Cancelled;

    public Result<RentalCost> CalculateCost(Vehicle vehicle)
    {
      if (IsCancelled())
        return Result<RentalCost>.Failure(
            Error.ValidationError("Cannot calculate cost for cancelled rental"));

      if (!IsCompleted())
        return Result<RentalCost>.Failure(
            Error.ValidationError("Cannot calculate cost for incomplete rental"));

      var distance = OdometerEnd!.Value - OdometerStart;
      var days = Math.Max(1, (int)Math.Ceiling((EndDate - StartDate).TotalDays));
      var batteryDelta = BatterySOCEnd!.Value - BatterySOCStart;

      var distanceCost = distance * vehicle.PricePerKmInEuro;
      var dailyCost = days * vehicle.PricePerDayInEuro;
      var batteryPenalty = Math.Max(0, -batteryDelta) * 0.2f;

      var totalCost = distanceCost + dailyCost + batteryPenalty;

      return Result<RentalCost>.Success(
          new RentalCost(distance, days, totalCost, distanceCost, dailyCost, batteryPenalty));
    }
    public Result<float> GetDistanceTraveled()
    {
      if (IsCancelled())
        return Result<float>.Failure(
            Error.ValidationError("Cannot calculate distance for cancelled rental"));

      // Technically incorrect, but leaving as-is for now... 2025-05-11 @ 08:03 PM
      if (!IsCompleted())
      return Result<float>.Failure(
          Error.ValidationError("Cannot calculate distance for incomplete rental"));

      var distance = OdometerEnd!.Value - OdometerStart;
      return Result<float>.Success(distance);
    }
    public bool OverlapsWith(DateTime otherStartDate, DateTime otherEndDate) =>
      StartDate <= otherEndDate && otherStartDate <= EndDate;

    public record RentalCost(
    float Distance,
    int Days,
    float TotalCost,
    float DistanceCost,
    float DailyCost,
    float BatteryPenalty);
  }
}