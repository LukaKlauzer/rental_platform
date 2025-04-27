namespace Core.DTOs.Rental
{
  public class RentalCreateDTO
  {
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string VehicleId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
  }
}
