using Core.DTOs.Customer;
using Core.Result;
using MediatR;

namespace Core.Features.Customer.Queries
{
  public record GetByIdCustomerQuery(int Id) : IRequest<Result<CustomerReturnSingleDTO>>;
}
