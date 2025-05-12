using FluentValidation;

namespace Application.Features.Customer.Queries.GetCustomerById
{
  internal class GetCustomerByIdValidator :AbstractValidator<GetCustomerByIdQuery>
  {
    public GetCustomerByIdValidator() 
    {
      RuleFor(c=>c.Id)
        .GreaterThan(0).WithMessage("Id must be greater than 0");
    }
  }
}