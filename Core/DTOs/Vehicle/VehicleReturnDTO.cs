namespace Core.DTOs.Vehicle
{
  public class VehicleReturnDTO
  {
    public string Vin { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateOnly Year { get; set; }
    public float PricePerKmInEuro { get; set; }
    public float PricePerDayInEuro { get; set; }
  }
}
