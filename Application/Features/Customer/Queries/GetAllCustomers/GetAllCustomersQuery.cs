using Application.DTOs.Customer;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Queries.GetAllCustomers
{
  public record GetAllCustomersQuery() : IRequest<Result<List<CustomerReturnDto>>>;

}
