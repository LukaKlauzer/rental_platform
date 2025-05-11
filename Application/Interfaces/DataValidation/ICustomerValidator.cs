using Application.DTOs.Customer;
using Core.Result;

namespace Application.Interfaces.DataValidation
{
  public interface ICustomerValidator
  {
    Result<bool> ValidateCreate(CustomerCreateDto dto);
    Result<bool> ValidateUpdate(CustomerUpdateDto dto);
    Result<bool> ValidateDelete(int id);
  }
}
