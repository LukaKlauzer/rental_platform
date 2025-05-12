using Application.DTOs.Customer;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Customer.Commands.DeleteCustomer
{

  internal class CustomerDeleteCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
  {
    private readonly ILogger<CustomerDeleteCommandHandler> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerMapper _customerMapper;

    public CustomerDeleteCommandHandler(
      ILogger<CustomerDeleteCommandHandler> logger,
      ICustomerRepository customerRepository,
      ICustomerMapper customerMapper)
    {
      _logger = logger;
      _customerRepository = customerRepository;
      _customerMapper = customerMapper;
    }

    public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
      var customerResult = await _customerRepository.GetByIdWithRentals(request.Id);
      if (customerResult.IsFailure)
        return Result<bool>.Failure(customerResult.Error);

      var customer = customerResult.Value;

      // If customer has rentals, soft delete
      if (customer.HasRentals())
      {
        _logger.LogInformation("Soft deleting customer {CustomerId} with {RentalCount} rentals",
            request.Id, customer.Rentals.Count);

        customer.MarkAsDeleted();

        var updateResult = await _customerRepository.SoftDelete(customer);
        return updateResult.Match(
            _ => Result<bool>.Success(true),
            error => Result<bool>.Failure(error));
      }

      // No rentals, hard delete
      _logger.LogInformation("Hard deleting customer {CustomerId} with no rentals", request.Id);

      var deleteResult = await _customerRepository.Delete(request.Id);
      return deleteResult.Match(
          _ => Result<bool>.Success(true),
          error => Result<bool>.Failure(error));
    }
  }
}
