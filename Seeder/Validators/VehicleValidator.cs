using Core.Domain.Entities;
using Core.Interfaces.Validation;

namespace Seeder.Validators
{
  internal class VehicleValidator : IVehicleValidator
  {
    public IEnumerable<Vehicle> FilterValidVehicles(IEnumerable<Vehicle> vehicles)
    {
      throw new NotImplementedException();
    }

    public bool IsValid(Vehicle vehicle)
    {
      throw new NotImplementedException();
    }
  }
}
