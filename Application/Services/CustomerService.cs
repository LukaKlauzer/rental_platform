using Application.DTOs.Customer;
using Application.Interfaces.Authentification;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Application.Interfaces.Services;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
  public class CustomerService : ICustomerService
  {
    private readonly ILogger<CustomerService> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRentalRepository _rentalRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerMapper _customerMapper;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public CustomerService(
      ILogger<CustomerService> logger,
      ICustomerRepository customerRepository,
      IRentalRepository rentalRepository,
      IVehicleRepository vehicleRepository,
      ICustomerMapper customerMapper,
      IJwtTokenGenerator jwtTokenGenerator)
    {
      _logger = logger;
      _customerRepository = customerRepository;
      _rentalRepository = rentalRepository;
      _vehicleRepository = vehicleRepository;
      _customerMapper = customerMapper;
      _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<CustomerReturnDto>> Create(CustomerCreateDto customerCreateDTO)
    {
      if (customerCreateDTO is null)
        return Result<CustomerReturnDto>.Failure(Error.NullReferenceError("CustomerReturnDto cannot be null."));

      if (string.IsNullOrEmpty(customerCreateDTO.Name))
      {
        _logger.LogWarning("Customer creation failed: Customer name was empty or null");
        return Result<CustomerReturnDto>.Failure(Error.ValidationError("Customer name can not be empty"));
      }

      var customerToCreateResult = _customerMapper.ToEntity(customerCreateDTO);
      if (customerToCreateResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(customerToCreateResult.Error);

      var newCustomerResult = await _customerRepository.Create(customerToCreateResult.Value);

      return newCustomerResult.Match(
        customer => _customerMapper.ToReturnDto(customer),
        error => Result<CustomerReturnDto>.Failure(error));
    }
    public async Task<Result<CustomerReturnDto>> Update(CustomerUpdateDto customerUpdateDto)
    {
      if (customerUpdateDto is null)
        return Result<CustomerReturnDto>.Failure(Error.NullReferenceError("Customer update data cannot be null"));

      if (customerUpdateDto.Id <= 0)
      {
        _logger.LogWarning("Customer update failed: Customer Id not valid");
        return Result<CustomerReturnDto>.Failure(Error.ValidationError("Invalid customer ID"));
      }
      if (string.IsNullOrEmpty(customerUpdateDto.Name))
      {
        _logger.LogWarning("Customer update failed: Customer name was null or empty");
        return Result<CustomerReturnDto>.Failure(Error.ValidationError("Customer name can not be empty"));
      }
      var customerResult =  await _customerRepository.GetById(customerUpdateDto.Id);
      if (customerResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(customerResult.Error);


      var updatedResult = customerResult.Value.Update(customerUpdateDto.Name);
      if (updatedResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(updatedResult.Error);


      var customerUpdatedResult = await _customerRepository.Update(customerResult.Value);

      return customerUpdatedResult.Match(
        customer => _customerMapper.ToReturnDto(customer),
        error => Result<CustomerReturnDto>.Failure(error));
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
    public async Task<Result<List<CustomerReturnDto>>> GetAll()
    {
      var allCustomers = await _customerRepository.GetAll();

      return allCustomers.Match(
        customers =>_customerMapper.ToReturnDtoList(customers),
        error => Result<List<CustomerReturnDto>>.Failure(error)
       );
    }
    public async Task<Result<CustomerReturnSingleDto>> GetById(int id)
    {
      // Get customer
      var customerResult = await _customerRepository.GetById(id);
      if (customerResult.IsFailure)
        return Result<CustomerReturnSingleDto>.Failure(customerResult.Error);

      // Convert now to handle cases with no rentals or incomplete ones
      var returnDtoResult = _customerMapper.ToReturnSingleDto(customerResult.Value);
      if (returnDtoResult.IsFailure)
        return returnDtoResult;

      // Get customer rentals
      var allRentals = await _rentalRepository.GetByCustomerId(id);
      if (allRentals.IsFailure)
        return Result<CustomerReturnSingleDto>.Failure(allRentals.Error);

      // Return early if no rentals
      if (!allRentals.Value.Any())
        return returnDtoResult;

      // Get all unique vehicles from rentals
      var vins = allRentals.Value.Select(x => x.VehicleId).Distinct().ToList();
      if (vins is null)
        return returnDtoResult;

      var vehiclesResult = await _vehicleRepository.GetByVins(vins);
      if (vehiclesResult.IsFailure)
        return Result<CustomerReturnSingleDto>.Failure(vehiclesResult.Error);

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
        return returnDtoResult;

      var totalDistanceDriven = rentalStats.Sum(s => s!.Distance);
      var totalPrice = rentalStats.Sum(s => s!.Cost);
      returnDtoResult = _customerMapper.ToReturnSingleDto(customerResult.Value, totalDistanceDriven, totalPrice);

      return returnDtoResult;
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