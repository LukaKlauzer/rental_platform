using Core.DTOs.Rental;
using Core.Result;
using MediatR;

namespace Core.Features.Rental.Queries
{
  public record GetByIdRentalQuery(int Id) : IRequest<Result<RentalReturnSingleDTO>>;
}
