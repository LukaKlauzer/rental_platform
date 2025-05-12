using Application.DTOs.Customer;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Commands.CreateCustomer
{
  public record CreateCustomerCommand(string Name) : IRequest<Result<CustomerReturnDto>>;

}
