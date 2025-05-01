using Core.DTOs.Customer;
using Core.DTOs.Rental;
using Core.Result;
using MediatR;

namespace Core.Features.Customer.Commands
{
  public record CreateCustomerCommand(string Name) : IRequest<Result<CustomerReturnDTO>>;
}
