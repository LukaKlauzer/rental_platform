using Core.DTOs.Customer;
using Core.Result;
using MediatR;

namespace Core.Features.Customer.Queries
{
  public record GetAllCustomerQuery : IRequest<Result<List<CustomerReturnDTO>>>;
}
