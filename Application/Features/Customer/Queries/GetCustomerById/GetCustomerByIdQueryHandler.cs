using Application.DTOs.Customer;
using Application.Interfaces.DataValidation;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;

namespace Application.Features.Customer.Queries.GetCustomerById
{
  internal class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerReturnSingleDto>>
  {
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerMapper _customerMapper;
    
    public GetCustomerByIdQueryHandler(
      ICustomerRepository customerRepository,
      IVehicleRepository vehicleRepository,
      ICustomerMapper customerMapper)
    {
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
      _customerMapper = customerMapper;
    }
  
    public async Task<Result<CustomerReturnSingleDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
      var customerResult = await _customerRepository.GetByIdWithRentals(request.Id);
      if (customerResult.IsFailure)
        return Result<CustomerReturnSingleDto>.Failure(customerResult.Error);

      var customer = customerResult.Value;

      if (!customer.HasRentals())
        return _customerMapper.ToReturnSingleDto(customer);

      // Get vehicles for completed rentals
      var completedRentals = customer.Rentals
          .Where(r => r.IsCompleted())
          .ToList();

      if (!completedRentals.Any())
        return _customerMapper.ToReturnSingleDto(customer);

      // Get unique vehicle VINs
      var vehicleIds = completedRentals
          .Select(r => r.VehicleId)
          .Distinct()
          .ToList();

      var vehiclesResult = await _vehicleRepository.GetByVins(vehicleIds);
      if (vehiclesResult.IsFailure)
        return Result<CustomerReturnSingleDto>.Failure(vehiclesResult.Error);

      var statisticsResult = customer.CalculateStatistics(
          vehiclesResult.Value);

      if (statisticsResult.IsFailure)
        return Result<CustomerReturnSingleDto>.Failure(statisticsResult.Error);

      var statistics = statisticsResult.Value;

      return _customerMapper.ToReturnSingleDto(
          customer,
          statistics.TotalDistanceDriven,
          statistics.TotalPrice);
    }
  }
}