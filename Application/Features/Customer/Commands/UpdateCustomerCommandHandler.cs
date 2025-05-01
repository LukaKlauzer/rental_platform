using Core.DTOs.Customer;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Features.Customer.Commands
{
  public record UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerReturnDTO>>
  {
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;
    private readonly ICustomerRepository _customerRepository;
    public UpdateCustomerCommandHandler(
      ILogger<UpdateCustomerCommandHandler> logger,
      ICustomerRepository customerRepository)
    {
      _logger = logger;
      _customerRepository = customerRepository;
    }
    public async Task<Result<CustomerReturnDTO>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
      if (request is null)
        return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("Customer update data cannot be null"));

      if (request.Id <= 0)
      {
        _logger.LogWarning("Customer update failed: Customer Id not valid");
        return Result<CustomerReturnDTO>.Failure(Error.ValidationError("Invalid customer ID"));
      }
      if (string.IsNullOrEmpty(request.Name))
      {
        _logger.LogWarning("Customer update failed: Customer name was null or empty");
        return Result<CustomerReturnDTO>.Failure(Error.ValidationError("Customer name can not be empty"));
      }

      var customer = request.ToCustomer();
      if (customer is null)
        return Result<CustomerReturnDTO>.Failure(Error.MappingError("Failed to map DTO to customer entity"));

      var customerResult = await _customerRepository.Update(customer);

      return customerResult.Match(
        customer =>
        {
          var returnDto = customer.ToReturnDto();
          if (returnDto is not null)
            return Result<CustomerReturnDTO>.Success(returnDto);
          return Result<CustomerReturnDTO>.Failure(Error.MappingError("Failed to map Customer entity to DTO"));
        },
        error => Result<CustomerReturnDTO>.Failure(error));

    }
  }

}
