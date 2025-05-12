using Core.Result;
using MediatR;

namespace Application.Features.Customer.Commands.DeleteCustomer
{
  public record DeleteCustomerCommand(int Id) : IRequest<Result<bool>>;

}