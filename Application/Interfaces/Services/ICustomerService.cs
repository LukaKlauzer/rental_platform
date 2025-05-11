using Application.DTOs.Customer;
using Core.Result;

namespace Application.Interfaces.Services
{
  public interface ICustomerService
  {
    public Task<Result<CustomerReturnDto>> Create(CustomerCreateDto customerCreateDTO);
    public Task<Result<string>> Login(int id);
    public Task<Result<CustomerReturnDto>> Update(CustomerUpdateDto customerUpdateDTO);
    public Task<Result<bool>> Delete(int id);
    public Task<Result<List<CustomerReturnDto>>> GetAll();
    public Task<Result<CustomerReturnSingleDto>> GetById(int id);
  }
}
