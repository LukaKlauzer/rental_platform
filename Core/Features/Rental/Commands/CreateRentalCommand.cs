using Core.DTOs.Rental;
using Core.Result;
using MediatR;

namespace Core.Features.Rental.Commands
{
  public record CreateRentalCommand(DateTime StartDate, DateTime EndDate, string VehicleId, int CustomerId) : IRequest<Result<RentalReturnDTO>>;
}
