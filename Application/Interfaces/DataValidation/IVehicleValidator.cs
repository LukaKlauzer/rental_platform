using Core.Result;

namespace Application.Interfaces.DataValidation
{
  public interface IVehicleValidator
  {

    Result<bool> ValidateGetByVin(string vin);
  }
}
