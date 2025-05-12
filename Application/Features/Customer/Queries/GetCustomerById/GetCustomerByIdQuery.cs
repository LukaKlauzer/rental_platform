using Application.DTOs.Customer;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Queries.GetCustomerById
{
  public record GetCustomerByIdQuery(int Id) : IRequest<Result<CustomerReturnSingleDto>>;

}
