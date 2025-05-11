using System.Text.RegularExpressions;
using Core.Result;

namespace Core.Domain.EntityValidation
{
  internal class VehicleValidator
  {
    internal static Result<bool> ValidateVehicle(string vin, string make, string model, int year, float pricePerKmInEuro, float pricePerDayInEuro)
    {
      if (string.IsNullOrEmpty(vin))
        return Result<bool>.Failure(Error.ValidationError("Vin can not be null or empty string"));

      var vinRegex = new Regex("^[A-HJ-NPR-Z0-9]{17}$", RegexOptions.IgnoreCase);
      if (!vinRegex.IsMatch(vin))
        return Result<bool>.Failure(Error.ValidationError("VIN format is invalid."));
      // TODO more VIN validations... or create value object... and do validation in there

      if (string.IsNullOrEmpty(make))
        return Result<bool>.Failure(Error.ValidationError("Make can not be null or empty string"));

      if (string.IsNullOrEmpty(model))
        return Result<bool>.Failure(Error.ValidationError("Model can not be null or empty string"));

      if (year < 0)
        return Result<bool>.Failure(Error.ValidationError("Year can not be negativee number"));

      if (pricePerKmInEuro < 0)
        return Result<bool>.Failure(Error.ValidationError("Price per km in euro can not be negativee number"));

      if (pricePerDayInEuro < 0)
        return Result<bool>.Failure(Error.ValidationError("Price per day in euro can not be negativee number"));

      return Result<bool>.Success(true);
    }
  }
}
