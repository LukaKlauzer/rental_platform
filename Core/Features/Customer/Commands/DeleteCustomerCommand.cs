using Core.Result;
using MediatR;

namespace Core.Features.Customer.Commands
{
  public record DeleteCustomerCommand(int Id) : IRequest<Result<bool>>;
}
