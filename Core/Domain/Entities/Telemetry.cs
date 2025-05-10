using Core.Domain.Common;
using Core.Enums;
using Core.Result;

namespace Core.Domain.Entities
{
  public class Telemetry : EntityID
  {
    private Telemetry() { }
    private Telemetry(TelemetryType telemetryType, float value, int timestamp, string vehicleId) 
    {
      Name = telemetryType;
      Value = value;
      Timestamp = timestamp;
      VehicleId = vehicleId;
    }
    public static Result<Telemetry> Create(TelemetryType telemetryType, float value, int timestamp, string vehicleId)
    {
      var validationResult = ValidateData(telemetryType, value, timestamp, vehicleId);
      if (validationResult.IsFailure)
        return Result<Telemetry>.Failure(validationResult.Error);

      var telemetry = new Telemetry(telemetryType, value, timestamp, vehicleId);

      return Result<Telemetry>.Success(telemetry);
    }

    private static Result<bool> ValidateData(TelemetryType telemetryType, float value, int timestamp, string vehicleId)
    {
      switch (telemetryType)
      {
        case TelemetryType.Unknown:
          return Result<bool>.Failure(Error.ValidationError("Telemetry type cannot be Unknown"));

        case TelemetryType.odometer:
          if (value < 0)
            return Result<bool>.Failure(Error.ValidationError("Odometer value cannot be negative"));
          break;

        case TelemetryType.battery_soc:
          if (value < 0)
            return Result<bool>.Failure(Error.ValidationError("Battery SOC value can not be negative"));

          if (value > 100)
            return Result<bool>.Failure(Error.ValidationError("Battery SOC value can not be over 100"));
          break;

        default:
          return Result<bool>.Failure(Error.ValidationError($"Validation for telemetry type {telemetryType} not implemented"));
      }

      if (timestamp < 0)
        return Result<bool>.Failure(Error.ValidationError("Timestamp value can not be negative"));

      if (string.IsNullOrEmpty(vehicleId))
        return Result<bool>.Failure(Error.ValidationError("Vehicle vin value can not be null or empty"));

      return Result<bool>.Success(true);
    }

    public TelemetryType Name { get; private set; }
    public float Value { get; private set; }
    public int Timestamp { get; private set; }

    public string VehicleId { get; private set; } = string.Empty;
    public Vehicle? Vehicle { get; private set; }
  }
}
