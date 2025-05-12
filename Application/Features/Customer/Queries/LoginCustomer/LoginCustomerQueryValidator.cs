using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Features.Customer.Queries.LoginCustomer
{
  public class LoginCustomerQueryValidator : AbstractValidator<LoginCustomerQuery>
  {
    private readonly ILogger<LoginCustomerQueryValidator> _logger;

    public LoginCustomerQueryValidator(ILogger<LoginCustomerQueryValidator> logger)
    {
      _logger = logger;

      _logger.LogInformation("LoginCustomerQueryValidator constructor called");

      RuleFor(c => c.Id)
          .GreaterThan(0).WithMessage("Id must be greater than 0");
    }
  }
}