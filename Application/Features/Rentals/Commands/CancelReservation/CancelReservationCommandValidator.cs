using FluentValidation;

namespace Application.Features.Rentals.Commands.CancelReservation
{
  public class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
  {
    public CancelReservationCommandValidator()
    {
      RuleFor(c => c.Id)
          .GreaterThan(0)
          .WithMessage("Reservation ID must be greater than zero");
    }
  }
}
