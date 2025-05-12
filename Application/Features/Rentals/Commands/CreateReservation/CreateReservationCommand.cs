using Application.DTOs.Rental;
using Core.Result;
using MediatR;

namespace Application.Features.Rentals.Commands.CreateReservation
{
  public record CreateReservationCommand(
        DateTime StartDate,
        DateTime EndDate,
        string VehicleId,
        int CustomerId) : IRequest<Result<RentalReturnDto>>;
}
