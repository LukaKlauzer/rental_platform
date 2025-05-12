using FluentValidation;

namespace Application.Features.Customer.Commands.CreateCustomer
{
  internal class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
  {
    public CreateCustomerCommandValidator()
    {
      RuleFor(c => c.Name)
          .NotEmpty().WithMessage("Name is required");
    }
  }
}
