using Application.DTOs.Customer;
using Application.Interfaces.Authentification;
using Application.Interfaces.DataValidation;
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
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerMapper _customerMapper;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICustomerValidator _customerValidator;

    public CustomerService(
      ILogger<CustomerService> logger,
      ICustomerRepository customerRepository,
      IVehicleRepository vehicleRepository,
      ICustomerMapper customerMapper,
      IJwtTokenGenerator jwtTokenGenerator,
      ICustomerValidator customerValidator)
    {
      _logger = logger;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
      _customerMapper = customerMapper;
      _jwtTokenGenerator = jwtTokenGenerator;
      _customerValidator = customerValidator;
    }

    public async Task<Result<CustomerReturnDto>> Create(CustomerCreateDto customerCreateDTO)
    {

      var validationResult = _customerValidator.ValidateCreate(customerCreateDTO);
      if (validationResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(validationResult.Error);

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
      var validationResult = _customerValidator.ValidateUpdate(customerUpdateDto);
      if (validationResult.IsFailure)
        return Result<CustomerReturnDto>.Failure(validationResult.Error);

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
      var validationResult = _customerValidator.ValidateDelete(id);
      if (validationResult.IsFailure)
        return validationResult;

      var customerResult = await _customerRepository.GetByIdWithRentals(id);
      if (customerResult.IsFailure)
        return Result<bool>.Failure(customerResult.Error);

      var customer = customerResult.Value;

      // If customer has rentals, soft delete
      if (customer.HasRentals())
      {
        _logger.LogInformation("Soft deleting customer {CustomerId} with {RentalCount} rentals",
            id, customer.Rentals.Count);

        customer.MarkAsDeleted();

        var updateResult = await _customerRepository.SoftDelete(customer);
        return updateResult.Match(
            _ => Result<bool>.Success(true),
            error => Result<bool>.Failure(error));
      }

      // No rentals, hard delete
      _logger.LogInformation("Hard deleting customer {CustomerId} with no rentals", id);

      var deleteResult = await _customerRepository.Delete(id);
      return deleteResult.Match(
          _ => Result<bool>.Success(true),
          error => Result<bool>.Failure(error));
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
      var customerResult = await _customerRepository.GetByIdWithRentals(id);
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
    public async Task<Result<string>> Login(int id)
    {
      var validationResult = _customerValidator.ValidateLogin(id);
      if (validationResult.IsFailure)
        return Result<string>.Failure(validationResult.Error);

      var customerResult = await _customerRepository.GetById(id);
      if (customerResult.IsFailure)
        return Result<string>.Failure(customerResult.Error);

      var token = _jwtTokenGenerator.GenerateToken(customerResult.Value.ID, customerResult.Value.Name);

      return Result<string>.Success(token);

    }

  }
}