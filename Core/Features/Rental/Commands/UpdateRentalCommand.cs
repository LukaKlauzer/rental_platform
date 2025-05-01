using Core.Result;
using MediatR;

namespace Core.Features.Rental.Commands
{
  public record UpdateRentalCommand(int Id, DateTime? StartDate, DateTime? EndDate) : IRequest<Result<bool>>;
}
