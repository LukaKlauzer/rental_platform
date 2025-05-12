using Application.DTOs.Customer;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Commands.UpdateCustomer
{
  internal class CustomerUpdateCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<CustomerReturnDto>>
  {
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerMapper _customerMapper;
    public CustomerUpdateCommandHandler(ICustomerRepository customerRepository, ICustomerMapper customerMapper)
    {
      _customerRepository = customerRepository;
      _customerMapper = customerMapper;
    }

    public async Task<Result<CustomerReturnDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
      var customerResult = await _customerRepository.GetById(request.Id);
      if (customerResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(customerResult.Error);

      var updatedResult = customerResult.Value.Update(request.Name);
      if (updatedResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(updatedResult.Error);

      var customerUpdatedResult = await _customerRepository.Update(customerResult.Value);

      return customerUpdatedResult.Match(
        customer => _customerMapper.ToReturnDto(customer),
        error => Result<CustomerReturnDto>.Failure(error));
    }
  }
}