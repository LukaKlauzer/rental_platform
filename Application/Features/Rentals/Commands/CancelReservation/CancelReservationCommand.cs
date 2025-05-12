using Core.Result;
using MediatR;

namespace Application.Features.Rentals.Commands.CancelReservation
{
  public record CancelReservationCommand(int Id) : IRequest<Result<bool>>;
}
