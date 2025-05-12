using FluentValidation;

namespace Application.Features.Rentals.Commands.UpdateReservation
{
  public class UpdateReservationCommandValidator : AbstractValidator<UpdateReservationCommand>
  {
    public UpdateReservationCommandValidator()
    {
      RuleFor(c => c.Id)
          .GreaterThan(0)
          .WithMessage("Reservation ID must be greater than zero");

      RuleFor(c => c)
          .Must(c => c.StartDate != null || c.EndDate != null)
          .WithMessage("At least one of StartDate or EndDate must be provided");

      When(c => c.StartDate.HasValue && c.EndDate.HasValue, () =>
      {
        RuleFor(c => c.EndDate)
            .GreaterThan(c => c.StartDate.Value)
            .WithMessage("End date must be after start date");
      });
    }
  }
}
