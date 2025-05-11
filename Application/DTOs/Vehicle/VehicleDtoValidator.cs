using Core.Result;
using IVehicleValidator = Application.Interfaces.DataValidation.IVehicleValidator;

namespace Application.DTOs.Vehicle
{
  public class VehicleDtoValidator : IVehicleValidator
  {
    public Result<bool> ValidateGetByVin(string vin)
    {
      if (string.IsNullOrEmpty(vin))
        return Result<bool>.Failure(Error.ValidationError($"Vehicle vin is not valid: {vin}"));
      return Result<bool>.Success(true);
    }
  }
}
