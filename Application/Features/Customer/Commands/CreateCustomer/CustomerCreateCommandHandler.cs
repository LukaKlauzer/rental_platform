using Application.DTOs.Customer;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Commands.CreateCustomer
{
  internal class CustomerCreateCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerReturnDto>>
  {
    private readonly ICustomerMapper   _customerMapper;
    private readonly ICustomerRepository _customerRepository;
    public CustomerCreateCommandHandler(
      ICustomerMapper customerMapper,
      ICustomerRepository customerRepository) 
    {
      _customerMapper = customerMapper;
      _customerRepository = customerRepository;
    }
    public async Task<Result<CustomerReturnDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken = default)
    {
      var customerToCreateResult = _customerMapper.ToEntity(request);
      if (customerToCreateResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(customerToCreateResult.Error);

      var newCustomerResult = await _customerRepository.Create(customerToCreateResult.Value);

      return newCustomerResult.Match(
        customer => _customerMapper.ToReturnDto(customer),
        error => Result<CustomerReturnDto>.Failure(error));
    }
  }
}
