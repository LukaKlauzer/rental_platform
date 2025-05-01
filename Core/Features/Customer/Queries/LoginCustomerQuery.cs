using Core.Result;
using MediatR;

namespace Core.Features.Customer.Queries
{
  public record LoginCustomerQuery(int Id) : IRequest<Result<string>>;
}
