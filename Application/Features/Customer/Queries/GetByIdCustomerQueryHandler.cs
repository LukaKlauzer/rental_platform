using Core.DTOs.Customer;
using Core.Features.Customer.Queries;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;

namespace Application.Features.Customer.Queries
{
  public class GetByIdCustomerQueryHandler : IRequestHandler<GetByIdCustomerQuery, Result<CustomerReturnSingleDTO>>
  {
    private readonly ICustomerRepository _customerRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public GetByIdCustomerQueryHandler(
      ICustomerRepository customerRepository,
      IRentalRepository rentalRepository,
      IVehicleRepository vehicleRepository
      ) 
    {
      _customerRepository = customerRepository;
      _rentalRepository = rentalRepository;
      _vehicleRepository = vehicleRepository;
    }
    public async Task<Result<CustomerReturnSingleDTO>> Handle(GetByIdCustomerQuery request, CancellationToken cancellationToken)
    {
      // Get customer
      var customerResult = await _customerRepository.GetById(request.Id);
      if (customerResult.IsFailure)
        return Result<CustomerReturnSingleDTO>.Failure(customerResult.Error);

      var returnDto = customerResult.Value.ToReturnSingleDto();
      if (returnDto is null)
        return Result<CustomerReturnSingleDTO>.Failure(Error.NullReferenceError("Customer mapping to DTO failed"));

      // Get customer rentals
      var allRentals = await _rentalRepository.GetByCustomerId(request.Id);
      if (allRentals.IsFailure)
        return Result<CustomerReturnSingleDTO>.Failure(allRentals.Error);

      // Return early if no rentals
      if (!allRentals.Value.Any())
        return Result<CustomerReturnSingleDTO>.Success(returnDto);

      // Get all unique vehicles from rentals
      var vins = allRentals.Value.Select(x => x.VehicleId).Distinct().ToList();
      if (vins is null)
        return Result<CustomerReturnSingleDTO>.Success(returnDto);

      var vehiclesResult = await _vehicleRepository.GetByVins(vins);
      if (vehiclesResult.IsFailure)
        return Result<CustomerReturnSingleDTO>.Failure(vehiclesResult.Error);

      var vehicleDict = vehiclesResult.Value.ToDictionary(v => v.Vin);

      var completedRentals = allRentals.Value
      .Where(r => r.OdometerEnd.HasValue && r.BatterySOCEnd.HasValue)
      .ToList();

      // Calculate statistics
      var rentalStats = completedRentals.Select(rental =>
      {
        if (!vehicleDict.TryGetValue(rental.VehicleId, out var vehicle))
          return null;

        float distance = rental.OdometerEnd!.Value - rental.OdometerStart;
        int days = Math.Max(1, (int)Math.Ceiling((rental.EndDate - rental.StartDate).TotalDays));
        float batteryDelta = rental.BatterySOCEnd!.Value - rental.BatterySOCStart;

        float distanceCost = distance * vehicle.PricePerKmInEuro;
        float dailyCost = days * vehicle.PricePerDayInEuro;
        float batteryPenalty = Math.Max(0, (-batteryDelta)) * 0.2f;

        return new
        {
          Distance = distance,
          Days = days,
          Cost = distanceCost + dailyCost + batteryPenalty,
          Vehicle = vehicle
        };
      }).Where(stat => stat != null).ToList();

      if (!rentalStats.Any())
        return Result<CustomerReturnSingleDTO>.Success(returnDto);

      returnDto.TotalDistanceDriven = rentalStats.Sum(s => s!.Distance);
      returnDto.TotalPrice = rentalStats.Sum(s => s!.Cost);

      return Result<CustomerReturnSingleDTO>.Success(returnDto);
    }
  }
}
