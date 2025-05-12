using FluentValidation;

namespace Application.Features.Customer.Commands.UpdateCustomer
{
  internal class CustomerUpdateCommandValidator : AbstractValidator<UpdateCustomerCommand>
  {
    public CustomerUpdateCommandValidator() 
    {
      RuleFor(u=>u.Id)
        .GreaterThan(0).WithMessage("Id must be greater than 0");

      RuleFor(u => u.Name)
        .NotEmpty().WithMessage("Name is required");
    }
  }
}