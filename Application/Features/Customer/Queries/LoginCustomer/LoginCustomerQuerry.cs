using Core.Result;
using MediatR;

namespace Application.Features.Customer.Queries.LoginCustomer
{
  public record LoginCustomerQuery(int Id) : IRequest<Result<string>>;

}
