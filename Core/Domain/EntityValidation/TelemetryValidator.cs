using Core.Enums;
using Core.Result;

namespace Core.Domain.EntityValidation
{
  internal class TelemetryValidator
  {
    internal static Result<bool> ValidateData(TelemetryType telemetryType, float value, int timestamp, string vehicleId)
    {
      switch (telemetryType)
      {
        case TelemetryType.Unknown:
          return Result<bool>.Failure(Error.ValidationError("Telemetry type cannot be Unknown"));

        case TelemetryType.odometer:
          if (value < 0)
            return Result<bool>.Failure(Error.ValidationError("Odometer value cannot be negativee"));
          break;

        case TelemetryType.battery_soc:
          if (value < 0)
            return Result<bool>.Failure(Error.ValidationError("Battery SOC value can not be negativee"));

          if (value > 100)
            return Result<bool>.Failure(Error.ValidationError("Battery SOC value can not be over 100"));
          break;

        default:
          return Result<bool>.Failure(Error.ValidationError($"Validation for telemetry type {telemetryType} not implemented"));
      }

      if (timestamp < 0)
        return Result<bool>.Failure(Error.ValidationError("Timestamp value can not be negativee"));

      if (string.IsNullOrEmpty(vehicleId))
        return Result<bool>.Failure(Error.ValidationError("Vehicle vin value can not be null or empty"));

      return Result<bool>.Success(true);
    }
  }
}
