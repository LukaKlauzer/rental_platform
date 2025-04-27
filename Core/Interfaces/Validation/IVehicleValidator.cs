using Core.Domain.Entities;

namespace Core.Interfaces.Validation
{
  public interface IVehicleValidator
  {
    bool IsValid(Vehicle vehicle);
    IEnumerable<Vehicle> FilterValidVehicles(IEnumerable<Vehicle> vehicles);
  }
}
