using Core.Domain.Common;

namespace Core.Domain.Entities
{
  public class Customer : EntityID
  {
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public ICollection<Rental> RentalRecords { get; set; } = new List<Rental>();

  }
}
