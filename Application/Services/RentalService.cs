using Application.DTOs.Rental;
using Application.Interfaces.DataValidation;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Enums;
using Core.Result;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
  public class RentalService : IRentalService
  {
    private readonly ILogger<RentalService> _logger;
    private readonly IRentalRepository _rentalRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRentalMapper _rentalMapper;
    private readonly IRentalValidator _rentalValidator;
    public RentalService(
      ILogger<RentalService> logger,
      IRentalRepository rentalRepository,
      ICustomerRepository customerRepository,
      IVehicleRepository vehicleRepository,
      ITelemetryRepository telemetryRepository,
      IRentalMapper rentalMapper,
      IRentalValidator rentalValidator
      )
    {
      _rentalRepository = rentalRepository;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
      _logger = logger;
      _rentalMapper = rentalMapper;
      _rentalValidator = rentalValidator;
    }

    public async Task<Result<RentalReturnDto>> CreateReservation(RentalCreateDto dto)
    {
      var validationResult = _rentalValidator.ValidateCreate(dto);
      if (validationResult.IsFailure)
        return Result<RentalReturnDto>.Failure(validationResult.Error);

      var customerResult = await _customerRepository.GetByIdWithRentals(dto.CustomerId);
      if (customerResult.IsFailure)
        return Result<RentalReturnDto>.Failure(customerResult.Error);

      var canCreateRentalResult = customerResult.Value.CanCreateRental(dto.StartDate, dto.EndDate);
      if (canCreateRentalResult.IsFailure)
        return Result<RentalReturnDto>.Failure(canCreateRentalResult.Error);

      var vehicleResult = await _vehicleRepository.GetByVinWithTelemetryAndRentals(dto.VehicleId);
      if (vehicleResult.IsFailure)
        return Result<RentalReturnDto>.Failure(vehicleResult.Error);

      if (!vehicleResult.Value.IsAvailableForRental(dto.StartDate, dto.EndDate))
        return Result<RentalReturnDto>.Failure(
            Error.ValidationError("Vehicle is not available for the requested dates"));

      var odometerStartResult = vehicleResult.Value.GetTelemetryAtTime(dto.StartDate, TelemetryType.odometer);
      if (odometerStartResult.IsFailure)
        return Result<RentalReturnDto>.Failure(odometerStartResult.Error);

      var batteryStartResult = vehicleResult.Value.GetTelemetryAtTime(dto.StartDate, TelemetryType.battery_soc);
      if (batteryStartResult.IsFailure)
        return Result<RentalReturnDto>.Failure(batteryStartResult.Error);

      var odometerEndResult = vehicleResult.Value.GetTelemetryAfterTime(dto.EndDate, TelemetryType.odometer);
      var batteryEndResult = vehicleResult.Value.GetTelemetryAfterTime(dto.EndDate, TelemetryType.battery_soc);

      var rentalResult = Rental.Create(
          dto.StartDate,
          dto.EndDate,
          odometerStartResult.Value.Value,
          batteryStartResult.Value.Value,
          dto.VehicleId,
          dto.CustomerId,
          odometerEndResult.IsSuccess ? odometerEndResult.Value.Value : null,
          batteryEndResult.IsSuccess ? batteryEndResult.Value.Value : null);

      if (rentalResult.IsFailure)
        return Result<RentalReturnDto>.Failure(rentalResult.Error);

      var savedRentalResult = await _rentalRepository.Create(rentalResult.Value);
      if (savedRentalResult.IsFailure)
        return Result<RentalReturnDto>.Failure(savedRentalResult.Error);

      return _rentalMapper.ToReturnDto(savedRentalResult.Value);
    }
    public async Task<Result<bool>> CancelReservation(int id)
    {
      var validationResult = _rentalValidator.ValidateCancle(id);
      if (validationResult.IsFailure)
        return validationResult;

      var rentalResult = await _rentalRepository.GetById(id);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      var canceledResult = rentalResult.Value.Cancel();
      if (canceledResult.IsFailure)
        return canceledResult;

      var updatedRentalResult = await _rentalRepository.Update(rentalResult.Value);

      return updatedRentalResult.Match(
        rental => Result<bool>.Success(true),
        error => Result<bool>.Failure(error)
        );
    }
    public async Task<Result<List<RentalReturnDto>>> GetAll()
    {
      var allRentalsResult = await _rentalRepository.GetAll();

      return allRentalsResult.Match(
        rentals => _rentalMapper.ToReturnDtoList(rentals),
        error => Result<List<RentalReturnDto>>.Failure(error)
      );
    }
    public async Task<Result<RentalReturnSingleDto>> GetById(int id)
    {
      var validationResult = _rentalValidator.ValidateGetById(id);
      if (validationResult.IsFailure)
        return Result<RentalReturnSingleDto>.Failure(validationResult.Error);

      var rentalResult = await _rentalRepository.GetById(id);
      if (rentalResult.IsFailure)
        return Result<RentalReturnSingleDto>.Failure(rentalResult.Error);

      var rental = rentalResult.Value;

      var distanceResult = rental.GetDistanceTraveled();
      if (distanceResult.IsFailure)
        return Result<RentalReturnSingleDto>.Failure(distanceResult.Error);

      float? distanceTraveled = distanceResult.IsSuccess ? distanceResult.Value : null;

      return _rentalMapper.ToReturnSingleDto(rental, distanceTraveled);
    }
    public async Task<Result<bool>> UpdateReservation(RentalUpdateDto dto)
    {
      var validationResult = _rentalValidator.ValidateUpdate(dto);
      if (validationResult.IsFailure)
        return validationResult;

      var rentalResult = await _rentalRepository.GetByIdWithCustomerAndVehicle(dto.Id);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      var customerResult = await _customerRepository.GetByIdWithRentals(
          rentalResult.Value.CustomerId);
      if (customerResult.IsFailure)
        return Result<bool>.Failure(customerResult.Error);

      var vehicleResult = await _vehicleRepository.GetByVinWithRentals(
          rentalResult.Value.VehicleId);
      if (vehicleResult.IsFailure)
        return Result<bool>.Failure(vehicleResult.Error);

      var updateResult = rentalResult.Value.UpdateDates(
          dto.StartDate,
          dto.EndDate,
          customerResult.Value,
          vehicleResult.Value);

      if (updateResult.IsFailure)
        return updateResult;

      var savedResult = await _rentalRepository.Update(rentalResult.Value);
      return savedResult.Match(
          rental => Result<bool>.Success(true),
          error => Result<bool>.Failure(error));
    }
  }
}