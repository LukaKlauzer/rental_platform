using Core.Enums;
using CsvHelper.Configuration.Attributes;

namespace Seeder.Data
{

  internal record TelemetryCsvDto
  {
    [Index(0)]
    [Name("vin")]
    public string VehicleId { get; init; } = string.Empty;
    [Index(1)]
    [Name("name")]
    public TelemetryType Name { get; init; }
    [Index(2)]
    [Name("value")]
    public float Value { get; init; }
    [Index(3)]
    [Name("timestamp")]
    public int Timestamp { get; init; }

    // Parameterless constructor for CsvHelper
    public TelemetryCsvDto() { }
  }
}
