using Core.Domain.Common;
using Core.Domain.EntityValidation;
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
      var validationResult = TelemetryValidator.ValidateData(telemetryType, value, timestamp, vehicleId);
      if (validationResult.IsFailure)
        return Result<Telemetry>.Failure(validationResult.Error);

      var telemetry = new Telemetry(telemetryType, value, timestamp, vehicleId);

      return Result<Telemetry>.Success(telemetry);
    }

  

    public TelemetryType Name { get; private set; }
    public float Value { get; private set; }
    public int Timestamp { get; private set; }

    public string VehicleId { get; private set; } = string.Empty;
    public Vehicle? Vehicle { get; private set; }
  }
}
