using Core.DTOs.Rental;
using Core.Result;
using MediatR;

namespace Core.Features.Rental.Queries
{
  public record GetAllRentalsQuery : IRequest<Result<List<RentalReturnDTO>>>;
}
