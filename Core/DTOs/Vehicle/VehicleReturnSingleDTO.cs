namespace Core.DTOs.Vehicle
{
  public class VehicleReturnSingleDTO : VehicleReturnDTO
  {
    public float TotalDistanceDriven { get; set; }
    public int TotalRentalCount { get; set; }
    public float TotalRentalIncome { get; set; }

  }
}
