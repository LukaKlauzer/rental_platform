using Core.DTOs.Customer;
using Core.Extensions;
using Core.Interfaces.Authentification;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Services;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
  public class CustomerSevice : ICustomerService
  {
    private readonly ILogger<CustomerSevice> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    public CustomerSevice(
      ILogger<CustomerSevice> logger,
      ICustomerRepository customerRepository,
      IRentalRepository rentalRepository,
      IVehicleRepository vehicleRepository,
      IJwtTokenGenerator jwtTokenGenerator)
    {
      _logger = logger;
      _customerRepository = customerRepository;
      _rentalRepository = rentalRepository;
      _vehicleRepository = vehicleRepository;
      _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<CustomerReturnDTO>> Create(CustomerCreateDTO customerCreateDTO)
    {
      if (customerCreateDTO is null)
        return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("CustomerCreateDTO cannot be null."));

      if (string.IsNullOrEmpty(customerCreateDTO.Name))
      {
        _logger.LogWarning("Customer creation failed: Customer name was empty or null");
        return Result<CustomerReturnDTO>.Failure(Error.ValidationError("Customer name can not be empty"));
      }

      var customerToCreate = customerCreateDTO.ToCustomer();
      if (customerToCreate is null)
        return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("Cusomer can not be null"));

      var newCustomerResult = await _customerRepository.Create(customerToCreate);

      return newCustomerResult.Match(
        customer =>
        {
          var returnDto = customer.ToReturnDto();
          if (returnDto is null)
            return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("Customer mapping to DTO failed"));
          return Result<CustomerReturnDTO>.Success(returnDto);
        },
        error => Result<CustomerReturnDTO>.Failure(error));
    }
    public async Task<Result<CustomerReturnDTO>> Update(CustomerUpdateDTO customerUpdateDTO)
    {
      if (customerUpdateDTO is null)
        return Result<CustomerReturnDTO>.Failure(Error.NullReferenceError("Customer update data cannot be null"));

      if (customerUpdateDTO.Id <= 0)
      {
        _logger.LogWarning("Customer update failed: Customer Id not valid");
        return Result<CustomerReturnDTO>.Failure(Error.ValidationError("Invalid customer ID"));
      }
      if (string.IsNullOrEmpty(customerUpdateDTO.Name))
      {
        _logger.LogWarning("Customer update failed: Customer name was null or empty");
        return Result<CustomerReturnDTO>.Failure(Error.ValidationError("Customer name can not be empty"));
      }

      var customer = customerUpdateDTO.ToCustomer();
      if (customer is null)
        return Result<CustomerReturnDTO>.Failure(Error.MappingError("Failed to map DTO to customer entity"));

      var customerResult = await _customerRepository.Update(customer);

      return customerResult.Match(
        customer =>
        {
          var returnDto = customer.ToReturnDto();
          if (returnDto is not null)
            return Result<CustomerReturnDTO>.Success(returnDto);
          return Result<CustomerReturnDTO>.Failure(Error.MappingError("Failed to map Customer entity to DTO"));
        },
        error => Result<CustomerReturnDTO>.Failure(error));
    }
    public async Task<Result<bool>> Delete(int id)
    {
      // Check if the customer exists
      var customerResult = await _customerRepository.GetById(id);
      if (customerResult.IsFailure)
        return Result<bool>.Failure(customerResult.Error);

      // Check if customer has any rentals
      var rentalsResult = await _rentalRepository.GetByCustomerId(id);
      if (rentalsResult.IsFailure)
        return Result<bool>.Failure(rentalsResult.Error);

      // If customer has rentals, perform soft delete
      if (rentalsResult.Value.Any())
      {
        _logger.LogInformation("Performing soft delete for customer {CustomerId} because they have {RentalCount} existing rentals",
          id, rentalsResult.Value.Count());
        var softDeleteResult = await _customerRepository.SoftDelete(id);
        if (softDeleteResult.IsFailure)
          return Result<bool>.Failure(softDeleteResult.Error);

        return Result<bool>.Success(true);
      }

      // If no rentals, we can permanently delete customer, not necessarily a good idea... but YOLO
      var deleteResult = await _customerRepository.Delete(id);

      return deleteResult.Match(
        success =>
        {
          _logger.LogInformation("Successfully deleted customer {CustomerId} permanently", id);
          return Result<bool>.Success(true);
        },
        error =>
        {
          _logger.LogError("Permanent delete failed for customer {CustomerId}: {ErrorMessage}",
            id, error.Message);
        return Result<bool>.Failure(error);
    });

    }
    public async Task<Result<List<CustomerReturnDTO>>> GetAll()
    {
      var allCustomers = await _customerRepository.GetAll();

      return allCustomers.Match(
        customers => Result<List<CustomerReturnDTO>>.Success(customers.ToListReturnDto()),
        error => Result<List<CustomerReturnDTO>>.Failure(error)
       );
    }
    public async Task<Result<CustomerReturnSingleDTO>> GetById(int id)
    {
      // Get customer
      var customerResult = await _customerRepository.GetById(id);
      if (customerResult.IsFailure)
        return Result<CustomerReturnSingleDTO>.Failure(customerResult.Error);

      var returnDto = customerResult.Value.ToReturnSingleDto();
      if (returnDto is null)
        return Result<CustomerReturnSingleDTO>.Failure(Error.NullReferenceError("Customer mapping to DTO failed"));

      // Get customer rentals
      var allRentals = await _rentalRepository.GetByCustomerId(id);
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

    public async Task<Result<string>> Login(int id)
    {
      if (id <= 0)
      {
        _logger.LogWarning("Customer update failed: Customer Id not valid");
        return Result<string>.Failure(Error.ValidationError("Invalid customer ID"));
      }

      var customerResult = await _customerRepository.GetById(id);
      if (customerResult.IsFailure)
        return Result<string>.Failure(customerResult.Error);

      var token = _jwtTokenGenerator.GenerateToken(customerResult.Value.ID, customerResult.Value.Name);

      return Result<string>.Success(token);

    }

  }
}