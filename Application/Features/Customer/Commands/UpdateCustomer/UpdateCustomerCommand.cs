using Application.DTOs.Customer;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Commands.UpdateCustomer
{
  public record UpdateCustomerCommand(int Id, string Name) : IRequest<Result<CustomerReturnDto>>;

}
