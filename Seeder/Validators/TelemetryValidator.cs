using Core.Domain.Entities;
using Core.Enums;
using Core.Interfaces.Validation;
using Microsoft.Extensions.Logging;

namespace Seeder.Validators
{
  internal class TelemetryValidator : ITelemetryValidator
  {
    private readonly ILogger<TelemetryValidator> _logger;

    public TelemetryValidator(ILogger<TelemetryValidator> logger)
    {
      _logger = logger;
    }
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

      // Additional validation for battery_soc (range 0-100)
      if (telemetry.Name == TelemetryType.battery_soc && (telemetry.Value < 0 || telemetry.Value > 100))
      {
        _logger.LogWarning("Battery SOC telemetry record skipped for VIN {VIN}: Value {Value} out of range (0-100)",
            telemetry.VehicleId, telemetry.Value);
        return false;
      }

      // Additional validation for odometer (non-negative value)
      if (telemetry.Name == TelemetryType.odometer && telemetry.Value < 0)
      {
        _logger.LogWarning("Odometer telemetry record skipped for VIN {VIN}: Negative value {Value}",
            telemetry.VehicleId, telemetry.Value);
        return false;
      }

      return true;
    }

    public IEnumerable<Telemetry> FilterValidTelemetry(IEnumerable<Telemetry> telemetries)
    {
      return telemetries.Where(IsValid).ToList();
    }
  }
}
