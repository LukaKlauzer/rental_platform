using FluentValidation;

namespace Application.Features.Rentals.Queries.GetReservationById
{
  public class GetReservationByIdQueryValidator : AbstractValidator<GetReservationByIdQuery>
  {
    public GetReservationByIdQueryValidator()
    {
      RuleFor(q => q.Id)
          .GreaterThan(0)
          .WithMessage("Reservation ID must be greater than zero");
    }
  }
}
