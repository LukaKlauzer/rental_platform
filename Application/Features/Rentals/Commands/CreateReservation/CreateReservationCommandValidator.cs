using FluentValidation;

namespace Application.Features.Rentals.Commands.CreateReservation
{
  public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
  {
    public CreateReservationCommandValidator()
    {
      RuleFor(c => c.CustomerId)
          .GreaterThan(0)
          .WithMessage("Customer ID must be greater than zero");

      RuleFor(c => c.VehicleId)
          .NotEmpty()
          .WithMessage("Vehicle ID cannot be empty");

      RuleFor(c => c.StartDate)
          .NotEmpty()
          .WithMessage("Start date is required");

      RuleFor(c => c.EndDate)
          .NotEmpty()
          .WithMessage("End date is required")
          .GreaterThan(c => c.StartDate)
          .WithMessage("End date must be after start date");
    }
  }
}
