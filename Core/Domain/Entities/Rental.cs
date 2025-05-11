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

    public Result<bool> UpdateDates(DateTime? startDate = null, DateTime? endDate = null)
    {
      if (startDate is null && endDate is null)
        return Result<bool>.Failure(Error.ValidationError("At least one date must be provided for update"));

      var originalStartDate = StartDate;
      var originalEndDate = EndDate;

      if (startDate.HasValue)
        StartDate = startDate.Value;

      if (endDate.HasValue)
        EndDate = endDate.Value;

      var validationResult = RentalValidator.ValidateDates(startDate: StartDate, endDate: EndDate);
      if (validationResult.IsFailure)
      {
        StartDate = originalStartDate;
        EndDate = originalEndDate;

        return validationResult;
      }

      return Result<bool>.Success(true);
    }

  }
}