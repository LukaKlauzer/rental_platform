using Core.Domain.Entities;
using Core.Interfaces.Persistence.GenericRepository;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;

namespace Infrastructure.Persistance.ConcreteRepositories
{
  internal class CustomerRepository : ICustomerRepository
  {
    private readonly IRepository<Customer> _customerRepository;
    public CustomerRepository(IRepository<Customer> customerRepository)
    {
      _customerRepository = customerRepository;
    }
    public async Task<Result<Customer>> Create(Customer customer, CancellationToken cancellationToken = default)
    {
      if (customer is null)
        return Result<Customer>.Failure(Error.NullReferenceError("Customer cannot be null"));

      try
      {
        var newCustomer = await _customerRepository.AddOrUpdateAsync(customer, cancellationToken);
        return Result<Customer>.Success(newCustomer);
      }
      catch (Exception ex)
      {
        return Result<Customer>.Failure(Error.DatabaseWriteError($"Failed to create customer: {ex.Message}"));
      }
    }
    public async Task<Result<Customer>> Update(Customer customer, CancellationToken cancellationToken = default)
    {
      if (customer is null)
        return Result<Customer>.Failure(Error.NullReferenceError("Customer cannot be null"));

      if (customer.ID <= 0)
        Result<Customer>.Failure(Error.ValidationError("Customer ID must be specified for updates"));
      try
      {
        var updatedCustomer = await _customerRepository.AddOrUpdateAsync(customer, cancellationToken);
        return Result<Customer>.Success(updatedCustomer);
      }
      catch (Exception ex)
      {
        return Result<Customer>.Failure(Error.DatabaseWriteError($"Error updating customer with id {customer.ID}: {ex.Message}"));
      }
    }
    public async Task<Result<bool>> Delete(int id, CancellationToken cancellationToken = default)
    {
      try
      {
        var deleted = await _customerRepository.DeleteAsync(id, cancellationToken);

        if (deleted)
          return Result<bool>.Success(true);

        return Result<bool>.Failure(Error.NotFound($"Customer with id {id} not found or could not be deleted"));
      }
      catch (Exception ex)
      {
        return Result<bool>.Failure(Error.DatabaseWriteError($"Failed to delete customer with id {id}: {ex.Message}"));
      }
    }
    public async Task<Result<bool>> SoftDelete(int id, CancellationToken cancellationToken = default)
    {
      try
      {
        // Get the customer to mark as deleted
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);

        if (customer is null)
          return Result<bool>.Failure(Error.NotFound($"Customer with id {id} not found"));

        // Mark as deleted
        customer.IsDeleted = true;

        await _customerRepository.AddOrUpdateAsync(customer, cancellationToken);

        return Result<bool>.Success(true);
      }
      catch (Exception ex)
      {
        return Result<bool>.Failure(Error.DatabaseWriteError($"Failed to soft delete customer with id {id}: {ex.Message}"));
      }
    }

    public async Task<Result<List<Customer>>> GetAll(CancellationToken cancellationToken = default)
    {
      try
      {
        var customers = await _customerRepository.GetAsync(cancellationToken);
        return Result<List<Customer>>.Success(customers.ToList());
      }
      catch (Exception ex)
      {
        return Result<List<Customer>>.Failure(Error.DatabaseReadError($"Failed to retrieve customers: {ex.Message}"));
      }
    }

    public async Task<Result<Customer>> GetById(int id, CancellationToken cancellationToken = default)
    {
      try
      {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is not null)
          return Result<Customer>.Success(customer);
        return Result<Customer>.Failure(Error.NotFound($"Customer with id {id} not found"));
      }
      catch (Exception ex)
      {
        return Result<Customer>.Failure(Error.DatabaseReadError($"Error retrieving customer with id {id}: {ex.Message}"));
      }

    }

  }
}
