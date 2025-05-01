using Core.Features.Customer.Queries;
using Core.Interfaces.Authentification;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Customer.Queries
{
  public class LoginCustomerQueryHandler : IRequestHandler<LoginCustomerQuery, Result<string>>
  {
    private readonly ILogger<LoginCustomerQueryHandler> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCustomerQueryHandler(
      ILogger<LoginCustomerQueryHandler> logger,
      ICustomerRepository customerRepository,
      IJwtTokenGenerator jwtTokenGenerator)
    {
      _logger = logger;
      _customerRepository = customerRepository;
      _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<string>> Handle(LoginCustomerQuery request, CancellationToken cancellationToken)
    {
      if (request.Id <= 0)
      {
        _logger.LogWarning("Customer update failed: Customer Id not valid");
        return Result<string>.Failure(Error.ValidationError("Invalid customer ID"));
      }

      var customerResult = await _customerRepository.GetById(request.Id);
      if (customerResult.IsFailure)
        return Result<string>.Failure(customerResult.Error);

      var token = _jwtTokenGenerator.GenerateToken(customerResult.Value.ID, customerResult.Value.Name);

      return Result<string>.Success(token);
    }

  }
}
