using Core.Domain.Common;

namespace Core.Domain.Entities
{
  public class Customer : EntityID
  {
    public string Name { get; set; } = string.Empty;

    public ICollection<Rental> RentalRecords { get; set; } = new List<Rental>();

  }
}
