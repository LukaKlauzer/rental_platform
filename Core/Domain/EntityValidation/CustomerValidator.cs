using Core.Result;

namespace Core.Domain.EntityValidation
{
  internal static class CustomerValidator
  {
    internal static Result<bool> ValidateCustomerData(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
        return Result<bool>.Failure(Error.ValidationError("Customer name cannot be null or empty"));

      return Result<bool>.Success(true);
    }
  }
}
