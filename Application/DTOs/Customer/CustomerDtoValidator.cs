using Application.Interfaces.DataValidation;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Application.DTOs.Customer
{
  public class CustomerDtoValidator : ICustomerValidator
  {
    private readonly ILogger<CustomerDtoValidator> _logger;
    public CustomerDtoValidator(ILogger<CustomerDtoValidator> logger)
    {
      _logger = logger;
    }
    public Result<bool> ValidateCreate(CustomerCreateDto dto)
    {

      if (dto is null)
        return Result<bool>.Failure(Error.NullReferenceError("CustomerReturnDto cannot be null."));

      if (string.IsNullOrEmpty(dto.Name))
      {
        _logger.LogWarning("Customer creation failed: Customer name was empty or null");
        return Result<bool>.Failure(Error.ValidationError("Customer name can not be empty"));
      }

      return Result<bool>.Success(true);
    }
    public Result<bool> ValidateUpdate(CustomerUpdateDto dto)
    {
      if (dto is null)
        return Result<bool>.Failure(Error.NullReferenceError("Customer update data cannot be null"));

      if (dto.Id <= 0)
      {
        _logger.LogWarning("Customer update failed: Customer Id not valid");
        return Result<bool>.Failure(Error.ValidationError("Invalid customer ID"));
      }
      if (string.IsNullOrEmpty(dto.Name))
      {
        _logger.LogWarning("Customer update failed: Customer name was null or empty");
        return Result<bool>.Failure(Error.ValidationError("Customer name can not be empty"));
      }
      return Result<bool>.Success(true);
    }
    public Result<bool> ValidateDelete(int id)
    {
      if (id <= 0)
      {
        _logger.LogWarning("Customer delete validation failed: invalid Customer ID {CustomerId}", id);
        return Result<bool>.Failure(Error.ValidationError("Invalid customer ID"));
      }
      return Result<bool>.Success(true);
    }
  }
}
