using Core.Enums;

namespace Core.DTOs.Rental
{
  public class RentalReturnDTO
  {
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public RentalStatus RentalStatus { get; set; }

    public string VehicleId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
  }
}
