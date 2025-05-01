using Core.DTOs.Customer;
using Core.Result;

namespace Core.Interfaces.Services
{
  public interface ICustomerService
  {
    public Task<Result<CustomerReturnDTO>> Create(CustomerCreateDTO customerCreateDTO);
    public Task<Result<string>> Login(int id);
    public Task<Result<CustomerReturnDTO>> Update(CustomerUpdateDTO customerUpdateDTO);
    public Task<Result<bool>> Delete(int id);
    public Task<Result<List<CustomerReturnDTO>>> GetAll();
    public Task<Result<CustomerReturnSingleDTO>> GetById(int id);
  }
}
