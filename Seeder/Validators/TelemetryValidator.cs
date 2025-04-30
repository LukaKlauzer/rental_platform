using Core.Domain.Entities;
using Core.Enums;
using Core.Interfaces.Validation;
using Microsoft.Extensions.Logging;

namespace Seeder.Validators
{
  internal class TelemetryValidator : ITelemetryValidator
  {
    private readonly ILogger<TelemetryValidator> _logger;

    private static Dictionary<string, (float Reading, int Timestamp)> _vinOdometerMap = new();
    public TelemetryValidator(ILogger<TelemetryValidator> logger)
    {
      _logger = logger;
    }

    public IEnumerable<Telemetry> FilterValidTelemetry(IEnumerable<Telemetry> telemetries) =>
      telemetries.Where(IsValid).ToList();
    public bool IsValid(Telemetry telemetry)
    {
      // Check if VIN is missing
      if (string.IsNullOrWhiteSpace(telemetry.VehicleId))
      {
        _logger.LogWarning("Telemetry record skipped: Missing VIN");
        return false;
      }

      // Check if telemetry type is valid
      if (telemetry.Name == TelemetryType.Unknown)
      {
        _logger.LogWarning("Telemetry record skipped for VIN {VIN}: Unknown telemetry type", telemetry.VehicleId);
        return false;
      }

      // Check timestamp (Unix timestamp should be positive)
      if (telemetry.Timestamp <= 0)
      {
        _logger.LogWarning("Telemetry record skipped for VIN {VIN}: Invalid timestamp {Timestamp}",
                telemetry.VehicleId, telemetry.Timestamp);
        return false;
      }

      // Validation for battery_soc (range 0-100)
      if (telemetry.Name == TelemetryType.battery_soc && (telemetry.Value < 0 || telemetry.Value > 100))
      {
        _logger.LogWarning("Battery SOC telemetry record skipped for VIN {VIN}: Value {Value} out of range (0-100)",
                telemetry.VehicleId, telemetry.Value);
        return false;
      }

      // Validation for odometer (non-negative value)
      if (telemetry.Name == TelemetryType.odometer && telemetry.Value < 0)
      {
        _logger.LogWarning("Odometer telemetry record skipped for VIN {VIN}: Negative value {Value}",
                telemetry.VehicleId, telemetry.Value);
        return false;
      }
      if (telemetry.Name == TelemetryType.odometer)
        if (!IsOdometerValid(telemetry.VehicleId, telemetry.Value, telemetry.Timestamp))
        {
          _logger.LogWarning("Odometer telemetry record skipped for VIN {VIN}: Failed odometer progression check at timestamp {Timestamp}",
                  telemetry.VehicleId, telemetry.Timestamp);
          return false;
        }
      return true;
    }


    private static bool IsOdometerValid(string vin, float newOdometerReading, int timestamp)
    {
      if (_vinOdometerMap.TryGetValue(vin, out var lastRecord))
      {
        // If the new timestamp is later, the odometer should not decrease
        if (timestamp > lastRecord.Timestamp && newOdometerReading < lastRecord.Reading)
          return false; // Suspicious rollback with newer timestamp

        // If the new timestamp is earlier, just check if the reading is consistent
        if (timestamp < lastRecord.Timestamp && newOdometerReading > lastRecord.Reading)
          return false; // Suspicious future reading with older timestamp
      }

      // Only update the map if the timestamp is newer than what we have
      if (!_vinOdometerMap.ContainsKey(vin) || timestamp >= _vinOdometerMap[vin].Timestamp)
        _vinOdometerMap[vin] = (newOdometerReading, timestamp);


      return true;
    }
  }
}
