using Core.DTOs.Customer;
using Core.Features.Customer.Commands;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Customer.Commands
{
  public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerReturnDTO>>
  {
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;
    public CreateCustomerCommandHandler(
      ILogger<CreateCustomerCommandHandler> logger,
      ICustomerRepository customerRepository)
    {
      _logger = logger;
      _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerReturnDTO>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
      if (request is null)
        return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("CustomerCreateDTO cannot be null."));

      if (string.IsNullOrEmpty(request.Name))
      {
        _logger.LogWarning("Customer creation failed: Customer name was empty or null");
        return Result<CustomerReturnDTO>.Failure(Error.ValidationError("Customer name can not be empty"));
      }

      var customerToCreate = request.ToCustomer();
      if (customerToCreate is null)
        return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("Cusomer can not be null"));

      var newCustomerResult = await _customerRepository.Create(customerToCreate);

      return newCustomerResult.Match(
        customer =>
        {
          var returnDto = customer.ToReturnDto();
          if (returnDto is null)
            return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("Customer mapping to DTO failed"));
          return Result<CustomerReturnDTO>.Success(returnDto);
        },
        error => Result<CustomerReturnDTO>.Failure(error));
    }
  }
}
