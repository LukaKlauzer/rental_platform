using Application.DTOs.Rental;
using Core.Result;
using MediatR;

namespace Application.Features.Rentals.Queries.GetAllReservations
{
  public record GetAllReservationsQuery : IRequest<Result<List<RentalReturnDto>>>;
}
