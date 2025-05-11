using Core.Domain.Common;
using Core.Enums;
using Core.Result;

namespace Core.Domain.Entities
{
  public class Rental : EntityID
  {
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
      var validateResult = ValidateData(startDate, endDate, odometerStart, batterySOCStart, vehicleId, customerId, odometerEnd, batterySOCEnd);
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
    private static Result<bool> ValidateData(
      DateTime startDate,
      DateTime endDate,
      float odometerStart,
      float batterySOCStart,
      string vehicleId,
      int customerId,
      float? odometerEnd = null,
      float? batterySOCEnd = null)
    {
      var validateDateResult = ValidateDates(startDate, endDate);
      if (validateDateResult.IsFailure)
        return validateDateResult;

      if (odometerStart < 0)
        return Result<bool>.Failure(Error.ValidationError("Odometer start reading can not be negativee"));

      if (batterySOCStart < 0 || batterySOCStart > 100)
        return Result<bool>.Failure(Error.ValidationError($"Battery SOC start reading must be between 0 and 100, value provided:{batterySOCStart}"));
      if (odometerEnd.HasValue)
      {
        if (odometerEnd.Value < 0)
          return Result<bool>.Failure(Error.ValidationError("Odometer end reading cannot be negativee"));

        if (odometerEnd.Value < odometerStart)
          return Result<bool>.Failure(Error.ValidationError("Odometer end reading must be greater than or equal to start reading"));
      }

      if (batterySOCEnd.HasValue && (batterySOCEnd.Value < 0 || batterySOCEnd > 100))
        return Result<bool>.Failure(Error.ValidationError($"Battery SOC end reading must be between 0 and 100, value provided:{batterySOCEnd}"));

      if (string.IsNullOrEmpty(vehicleId))
        return Result<bool>.Failure(Error.ValidationError("Vehicle vin can not be null or empty"));

      if (customerId <= 0)
        return Result<bool>.Failure(Error.ValidationError("Customer id can not be negative value or zero"));

      return Result<bool>.Success(true);
    }

    private static Result<bool> ValidateDates(DateTime startDate, DateTime endDate)
    {
      if (endDate <= startDate)
        return Result<bool>.Failure(Error.ValidationError(" End date mush be after start date"));

      return Result<bool>.Success(true);
    }

    public Result<bool> UpdateDates(DateTime? startDate = null, DateTime? endDate = null)
    {
      if (startDate is  null && endDate is null)
        return Result<bool>.Failure(Error.ValidationError("At least one date must be provided for update"));

      var originalStartDate = StartDate;
      var originalEndDate = EndDate;

      if (startDate.HasValue)
        StartDate = startDate.Value;

      if (endDate.HasValue)
        EndDate = endDate.Value;

      var validationResult = ValidateDates(startDate: StartDate, endDate: EndDate);
      if (validationResult.IsFailure)
      {
        StartDate = originalStartDate;
        EndDate = originalEndDate;

        return validationResult;
      }

      return Result<bool>.Success(true);
    }
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
  }
}