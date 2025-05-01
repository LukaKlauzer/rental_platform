using Core.Result;
using MediatR;

namespace Core.Features.Rental.Commands
{
  public record CancelRentalCommand(int Id): IRequest<Result<bool>>;
}
