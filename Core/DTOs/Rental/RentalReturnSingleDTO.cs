namespace Core.DTOs.Rental
{
  public class RentalReturnSingleDTO : RentalReturnDTO
  {
    public float DistanceTraveled { get; set; }
    public float BatterySOCSAtStart { get; set; }
    public float BatterySOCAtEnd { get; set; }
  }
}
