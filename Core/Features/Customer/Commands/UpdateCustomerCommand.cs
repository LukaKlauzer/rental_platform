using Core.DTOs.Customer;
using Core.Result;
using MediatR;

namespace Core.Features.Customer.Commands
{
  public record UpdateCustomerCommand(int Id, string Name) : IRequest<Result<CustomerReturnDTO>>;
}
