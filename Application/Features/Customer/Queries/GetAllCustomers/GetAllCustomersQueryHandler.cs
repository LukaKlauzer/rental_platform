using Application.DTOs.Customer;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Queries.GetAllCustomers
{
  internal class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<List<CustomerReturnDto>>>
  {
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerMapper _customerMapper;
    public GetAllCustomersQueryHandler(
      ICustomerRepository customerRepository,
      ICustomerMapper customerMapper)
    {
      _customerRepository = customerRepository;
      _customerMapper = customerMapper;
    }
    public async Task<Result<List<CustomerReturnDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
      var allCustomers = await _customerRepository.GetAll();

      return allCustomers.Match(
        customers => _customerMapper.ToReturnDtoList(customers),
        error => Result<List<CustomerReturnDto>>.Failure(error)
       );
    }
  }
}
