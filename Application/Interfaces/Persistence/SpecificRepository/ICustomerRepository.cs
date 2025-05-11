using Core.Domain.Entities;
using Core.Result;

namespace Application.Interfaces.Persistence.SpecificRepository
{
  public interface ICustomerRepository
  {
    public Task<Result<Customer>> Create(Customer customer, CancellationToken cancellationToken = default);
    public Task<Result<Customer>> Update(Customer customer, CancellationToken cancellationToken = default);
    public Task<Result<bool>> Delete(int id, CancellationToken cancellationToken = default);
    public Task<Result<bool>> SoftDelete(Customer customer, CancellationToken cancellationToken = default);
    public Task<Result<List<Customer>>> GetAll(CancellationToken cancellationToken = default);
    public Task<Result<Customer>> GetById(int id, CancellationToken cancellationToken = default);
    public Task<Result<Customer>> GetByIdWithRentals(int id, CancellationToken cancellationToken = default);
    
  }
}
