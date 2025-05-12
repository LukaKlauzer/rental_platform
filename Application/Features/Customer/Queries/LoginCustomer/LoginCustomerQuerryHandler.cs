using Application.Interfaces.Authentification;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Queries.LoginCustomer
{
  internal class LoginCustomerqueryHandler : IRequestHandler<LoginCustomerQuery, Result<string>>
  {
    private readonly ICustomerRepository _customerRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    public LoginCustomerqueryHandler(ICustomerRepository customerRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
      _customerRepository = customerRepository;
      _jwtTokenGenerator = jwtTokenGenerator;
    } 

    public async Task<Result<string>> Handle(LoginCustomerQuery request, CancellationToken cancellationToken)
    {
      var customerResult = await _customerRepository.GetById(request.Id);
      if (customerResult.IsFailure)
        return Result<string>.Failure(customerResult.Error);

      var token = _jwtTokenGenerator.GenerateToken(customerResult.Value.ID, customerResult.Value.Name);

      return Result<string>.Success(token);
    }
  }
}
