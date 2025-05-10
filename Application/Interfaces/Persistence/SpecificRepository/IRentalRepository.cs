using Core.Domain.Entities;
using Core.Result;

namespace Application.Interfaces.Persistence.SpecificRepository
{
  public interface IRentalRepository
  {
    public Task<Result<Rental>> Create(Rental rental, CancellationToken cancellationToken = default);
    public Task<Result<Rental>> Update(Rental rental, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<Rental>>> GetAll(CancellationToken cancellationToken = default);
    public Task<Result<Rental>> GetById(int id, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<Rental>>> GetByCustomerId(int customerId, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<Rental>>> GetByVin(string vin, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<Rental>>> GetByCustomerIdInTimeFrame(int customerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<Rental>>> GetByVinInTimeFrame(string vin, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
  }
}
