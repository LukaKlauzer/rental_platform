using CsvHelper.Configuration.Attributes;

namespace Seeder.Data
{
  internal record VehicleCsvDto
  {
    [Index(0)]
    [Name("vin")]
    public string Vin { get; init; } = string.Empty;
    [Index(1)]
    [Name("make")]
    public string Make { get; init; } = string.Empty;
    [Index(2)]
    [Name("model")]
    public string Model { get; init; } = string.Empty;
    [Index(3)]
    [Name("year")]
    public int Year { get; init; }
    [Index(4)]
    [Name("pricePerKmInEuro")]
    public float PricePerKmInEuro { get; init; }
    [Index(5)]
    [Name("pricePerDayInEuro")]
    public float PricePerDayInEuro { get; init; }

    // Parameterless constructor for CsvHelper
    public VehicleCsvDto() { }
  }
}
