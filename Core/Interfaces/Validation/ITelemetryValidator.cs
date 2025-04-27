using Core.Domain.Entities;

namespace Core.Interfaces.Validation
{
  public interface ITelemetryValidator
  {
    bool IsValid(Telemetry telemetry);
    IEnumerable<Telemetry> FilterValidTelemetry(IEnumerable<Telemetry> telemetries);
  }
}