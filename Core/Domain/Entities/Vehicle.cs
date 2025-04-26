using Core.Domain.Common;

namespace Core.Domain.Entities
{
  public class Vehicle : Entity
  {
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateOnly Year { get; set; }
    public float PricePerKmInEuro { get; set; }
    public float PricePerDayInEuro { get; set; }

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public ICollection<Telemetry> TelemetryRecords { get; set; } = new List<Telemetry>();
  }
}
