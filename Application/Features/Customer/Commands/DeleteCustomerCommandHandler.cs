using Core.DTOs.Customer;
using Core.Features.Customer.Commands;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Customer.Commands
{
  public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>
  {
    private readonly ILogger<DeleteCustomerCommandHandler> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRentalRepository _rentalRepository;

    public DeleteCustomerCommandHandler(
      ILogger<DeleteCustomerCommandHandler> logger,
      ICustomerRepository customerRepository,
      IRentalRepository rentalRepository)
    {
      _logger = logger;
      _customerRepository = customerRepository;
      _rentalRepository = rentalRepository;
    }

    public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
      // Check if the customer exists
      var customerResult = await _customerRepository.GetById(request.Id);
      if (customerResult.IsFailure)
        return Result<bool>.Failure(customerResult.Error);

      // Check if customer has any rentals
      var rentalsResult = await _rentalRepository.GetByCustomerId(request.Id);
      if (rentalsResult.IsFailure)
        return Result<bool>.Failure(rentalsResult.Error);

      // If customer has rentals, perform soft delete
      if (rentalsResult.Value.Any())
      {
        _logger.LogInformation("Performing soft delete for customer {CustomerId} because they have {RentalCount} existing rentals",
          request.Id, rentalsResult.Value.Count());
        var softDeleteResult = await _customerRepository.SoftDelete(request.Id);
        if (softDeleteResult.IsFailure)
          return Result<bool>.Failure(softDeleteResult.Error);

        return Result<bool>.Success(true);
      }

      // If no rentals, we can permanently delete customer, not necessarily a good idea... but YOLO
      var deleteResult = await _customerRepository.Delete(request.Id);

      return deleteResult.Match(
        success =>
        {
          _logger.LogInformation("Successfully deleted customer {CustomerId} permanently", request.Id);
          return Result<bool>.Success(true);
        },
        error =>
        {
          _logger.LogError("Permanent delete failed for customer {CustomerId}: {ErrorMessage}",
            request.Id, error.Message);
          return Result<bool>.Failure(error);
        });
    }
  }
}
