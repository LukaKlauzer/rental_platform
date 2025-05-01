using Core.DTOs.Customer;
using Core.Features.Customer.Queries;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;

namespace Application.Features.Customer.Queries
{
  public class GetAllCustomerQueryHandler : IRequestHandler<GetAllCustomerQuery, Result<List<CustomerReturnDTO>>>
  {
    private readonly ICustomerRepository _customerRepository;
    public GetAllCustomerQueryHandler(ICustomerRepository customerRepository)
    {
      _customerRepository = customerRepository;
    }
    public async Task<Result<List<CustomerReturnDTO>>> Handle(GetAllCustomerQuery request, CancellationToken cancellationToken)
    {
      var allCustomers = await _customerRepository.GetAll();

      return allCustomers.Match(
        customers => Result<List<CustomerReturnDTO>>.Success(customers.ToListReturnDto()),
        error => Result<List<CustomerReturnDTO>>.Failure(error)
       );
    }
  }
}
