using FluentValidation;

namespace Application.Features.Customer.Commands.DeleteCustomer
{
  internal class CustomerDeleteCommandValidator : AbstractValidator<DeleteCustomerCommand>
  {
    public CustomerDeleteCommandValidator() 
    {
      RuleFor(c => c.Id)
        .GreaterThan(0).WithMessage("Id must be greater than 0");
    }
  }
}