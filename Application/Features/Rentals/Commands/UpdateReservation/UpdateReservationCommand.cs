using Core.Result;
using MediatR;

namespace Application.Features.Rentals.Commands.UpdateReservation
{
  public record UpdateReservationCommand(
       int Id,
       DateTime? StartDate,
       DateTime? EndDate) : IRequest<Result<bool>>;
}
