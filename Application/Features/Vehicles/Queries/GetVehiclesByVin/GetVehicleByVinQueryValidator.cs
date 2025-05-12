using FluentValidation;

namespace Application.Features.Vehicles.Queries.GetVehiclesByVin
{
  public class GetVehicleByVinQueryValidator : AbstractValidator<GetVehicleByVinQuery>
  {
    public GetVehicleByVinQueryValidator()
    {
      RuleFor(q => q.Vin)
          .NotEmpty()
          .WithMessage("VIN cannot be empty")
          .MaximumLength(17)
          .WithMessage("VIN cannot exceed 17 characters")
          .MinimumLength(17)
          .WithMessage("VIN must be 17 characters long")
          .Matches(@"^[A-HJ-NPR-Z0-9]{17}$")
          .WithMessage("VIN contains invalid characters");
    }
  }
}