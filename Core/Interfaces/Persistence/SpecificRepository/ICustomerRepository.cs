using Core.Domain.Entities;
using Core.Result;

namespace Core.Interfaces.Persistence.SpecificRepository
{
  public interface ICustomerRepository
  {
    public Task<Result<Customer>> Create(Customer customer, CancellationToken cancellationToken = default);
    public Task<Result<Customer>> Update(Customer customer, CancellationToken cancellationToken = default);
    public Task<Result<List<Customer>>> GetAll(CancellationToken cancellationToken = default);
    public Task<Result<Customer>> GetById(int id, CancellationToken cancellationToken = default);
  }
}
