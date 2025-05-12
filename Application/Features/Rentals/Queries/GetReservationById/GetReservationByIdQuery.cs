using Application.DTOs.Rental;
using Core.Result;
using MediatR;

namespace Application.Features.Rentals.Queries.GetReservationById
{
  public record GetReservationByIdQuery(int Id) : IRequest<Result<RentalReturnSingleDto>>;
}
