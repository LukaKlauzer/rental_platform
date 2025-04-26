using Core.Domain.Common;
using Core.Enums;

namespace Core.Domain.Entities
{
  public class Telemetry : EntityID
  {
    public TelemetryType Name { get; set; }
    public int Value { get; set; }
    public int Timestamp { get; set; }

    public string VehicleId { get; set; } = string.Empty;
    public Vehicle? Vehicle { get; set; }
  }
}
