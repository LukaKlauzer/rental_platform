using Application.DTOs.Customer;
using Core.Domain.Entities;
using Core.Result;

namespace Application.Interfaces.Mapers
{
  public interface ICustomerMapper
  {
    Result<Customer> ToEntity(CustomerCreateDto dto);
    Result<CustomerReturnDto> ToReturnDto(Customer entity);
    Result<CustomerReturnSingleDto> ToReturnSingleDto(Customer entity, float totalDistanceDriven = 0f, float totalPrice = 0f);
    Result<List<CustomerReturnDto>> ToReturnDtoList(IEnumerable<Customer> entities);
  }
}
