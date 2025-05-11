using Core.Result;

namespace Core.Domain.EntityValidation
{
  internal static class RentalValidator
  {
    internal static Result<bool> ValidateData(
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

    internal static Result<bool> ValidateDates(DateTime startDate, DateTime endDate)
    {
      if (endDate <= startDate)
        return Result<bool>.Failure(Error.ValidationError(" End date mush be after start date"));

      return Result<bool>.Success(true);
    }
  }
}
