using Core.Domain.Common;
using Core.Enums;

namespace Core.Domain.Entities
{
  public class Rental : EntityID
  {
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public RentalStatus RentalStatus { get; set; }

    public string VehicleId { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int TelemetryId { get; set; }

    public Vehicle Vehicle { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
  }
}
