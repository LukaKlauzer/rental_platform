using Application.DTOs.Rental;
using Core.Enums;
using Application.Interfaces.Persistence.SpecificRepository;
using Application.Interfaces.Services;
using Core.Result;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Mapers;

namespace Application.Services
{
  public class RentalService : IRentalService
  {
    private readonly ILogger<RentalService> _logger;
    private readonly IRentalRepository _rentalRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IRentalMapper _rentalMapper;
    public RentalService(
      ILogger<RentalService> logger,
      IRentalRepository rentalRepository,
      ICustomerRepository customerRepository,
      IVehicleRepository vehicleRepository,
      ITelemetryRepository telemetryRepository,
      IRentalMapper rentalMapper
      )
    {
      _rentalRepository = rentalRepository;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
      _telemetryRepository = telemetryRepository;
      _logger = logger;
      _rentalMapper = rentalMapper;
    }

    public async Task<Result<RentalReturnDto>> CreateReservation(RentalCreateDto rentalCreateDTO)
    {
      if (rentalCreateDTO.CustomerId <= 0)
        return Result<RentalReturnDto>.Failure(Error.ValidationError($"Customer id is not valid: {rentalCreateDTO.CustomerId}"));

      if (string.IsNullOrEmpty(rentalCreateDTO.VehicleId))
        return Result<RentalReturnDto>.Failure(Error.ValidationError($"Vehicle vin is not valid: {rentalCreateDTO.VehicleId}"));

      // Get customer
      var customerResult = await _customerRepository.GetById(rentalCreateDTO.CustomerId);
      if (customerResult.IsFailure)
      {
        _logger.LogWarning("Failed to retrieve customer {CustomerId} for rental creation: {ErrorMessage}",
          rentalCreateDTO.CustomerId, customerResult.Error.Message);
        return Result<RentalReturnDto>.Failure(customerResult.Error);
      }
      if (customerResult.Value.IsDeleted == true)
        return Result<RentalReturnDto>.Failure(Error.ValidationError("Customer is no longer active or has been deleted."));

      // Get vehicle
      var vehicleResult = await _vehicleRepository.GetByVin(rentalCreateDTO.VehicleId);
      if (vehicleResult.IsFailure)
      {
        _logger.LogWarning("Failed to retrieve vehicle {VehicleId} for rental creation: {ErrorMessage}",
          rentalCreateDTO.VehicleId, customerResult.Error.Message);
        return Result<RentalReturnDto>.Failure(vehicleResult.Error);
      }

      var overlappingReservation = await IsOverlappingReservation(
      rentalCreateDTO.CustomerId,
      rentalCreateDTO.VehicleId,
      rentalCreateDTO.StartDate,
      rentalCreateDTO.EndDate);

      if (overlappingReservation.IsFailure)
      {
        _logger.LogWarning("Overlapping reservation detected for customer {CustomerId}, vehicle {VehicleId}, dates {StartDate:d} to {EndDate:d}: {ErrorMessage}",
            rentalCreateDTO.CustomerId, rentalCreateDTO.VehicleId, rentalCreateDTO.StartDate, rentalCreateDTO.EndDate,
            overlappingReservation.Error.Message);
        return Result<RentalReturnDto>.Failure(overlappingReservation.Error);
      }

      var odometerStartResult = await _telemetryRepository.GetMostRecentBefore(rentalCreateDTO.VehicleId, rentalCreateDTO.StartDate, TelemetryType.odometer);
      if (odometerStartResult.IsFailure)
        return Result<RentalReturnDto>.Failure(odometerStartResult.Error);

      var odometerEndResult = await _telemetryRepository.GetEarliestAfter(rentalCreateDTO.VehicleId, rentalCreateDTO.EndDate, TelemetryType.odometer);

      var batterySocStartResult = await _telemetryRepository.GetMostRecentBefore(rentalCreateDTO.VehicleId, rentalCreateDTO.StartDate, TelemetryType.battery_soc);
      if (batterySocStartResult.IsFailure)
        return Result<RentalReturnDto>.Failure(batterySocStartResult.Error);

      var batterySocEndResult = await _telemetryRepository.GetEarliestAfter(rentalCreateDTO.VehicleId, rentalCreateDTO.EndDate, TelemetryType.battery_soc);



      // Set the mandatory start values
      var odometerStart = odometerStartResult.Value.Value;
      var batterySOCStart = batterySocStartResult.Value.Value;
      float odometerEnd = 0, batterySOCEnd = 0;
      // Set end values if available
      if (odometerEndResult.IsSuccess)
        odometerEnd = odometerEndResult.Value.Value;

      if (batterySocEndResult.IsSuccess)
        batterySOCEnd = batterySocEndResult.Value.Value;

      var rentalToCreateResult = _rentalMapper.ToEntity(rentalCreateDTO, odometerStart, batterySOCStart, odometerEnd, batterySOCEnd);
      if (rentalToCreateResult.IsFailure)
        return Result<RentalReturnDto>.Failure(rentalToCreateResult.Error);

      var newReservation = await _rentalRepository.Create(rentalToCreateResult.Value);
      return newReservation.Match(
        reservation =>
        {
          var returnDtoResult = _rentalMapper.ToReturnDto(reservation);
          if (returnDtoResult.IsSuccess)
          {
            _logger.LogInformation("Successfully created rental reservation with ID: {RentalId} for customer {CustomerId}, vehicle {VehicleId}, from {StartDate:d} to {EndDate:d}",
              reservation.ID, reservation.CustomerId, reservation.VehicleId, reservation.StartDate, reservation.EndDate);
            return Result<RentalReturnDto>.Success(returnDtoResult.Value);
          }
          return Result<RentalReturnDto>.Failure(Error.MappingError($"Failed to map reservation entity to reservation DTO: {returnDtoResult.Error}"));
        },
        error =>
        {
          _logger.LogError("Failed to create rental reservation: {ErrorMessage}", error.Message);
          return Result<RentalReturnDto>.Failure(error);
        }
        );
    }
    public async Task<Result<bool>> CancelReservation(int id)
    {
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
      var rentalResult = await _rentalRepository.GetById(id);
      if (rentalResult.IsFailure)
        return Result<RentalReturnSingleDto>.Failure(rentalResult.Error);

      float? distanceTraveled = null;
      if (rentalResult.Value.OdometerEnd.HasValue)
        distanceTraveled = rentalResult.Value.OdometerEnd.Value - rentalResult.Value.OdometerStart;

      var returnDtoResult = _rentalMapper.ToReturnSingleDto(rentalResult.Value, distanceTraveled);
      if (returnDtoResult.IsFailure)
        return Result<RentalReturnSingleDto>.Failure(returnDtoResult.Error);

      return Result<RentalReturnSingleDto>.Success(returnDtoResult.Value);
    }

    public async Task<Result<bool>> UpdateReservation(RentalUpdateDto rentalUpdateDTO)
    {
      if (rentalUpdateDTO.Id <= 0)
      {
        _logger.LogWarning("Rental update validation failed: Invalid ID ({RentalId})", rentalUpdateDTO.Id);
        return Result<bool>.Failure(Error.ValidationError($"Rental id is not valid: {rentalUpdateDTO.Id}"));
      }
      if (rentalUpdateDTO.StartDate is null && rentalUpdateDTO.EndDate is null)
      {
        _logger.LogWarning("Rental update validation failed: No update fields provided for rental {RentalId}", rentalUpdateDTO.Id);
        return Result<bool>.Failure(Error.ValidationError("At least one field must be provided for update"));
      }
      if (!rentalUpdateDTO.StartDate.HasValue && !rentalUpdateDTO.EndDate.HasValue)
        return Result<bool>.Failure(Error.ValidationError("Both start date and end date cannot be null for reservation update"));

      var rentalResult = await _rentalRepository.GetById(rentalUpdateDTO.Id);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      // update on object level, not persistence
      var updatedDatesResult = rentalResult.Value.UpdateDates(rentalUpdateDTO.StartDate, rentalUpdateDTO.EndDate);
      if (updatedDatesResult.IsFailure)
        return updatedDatesResult;

      var overlappingReservation = await IsOverlappingReservation(
        rentalResult.Value.CustomerId,
        rentalResult.Value.VehicleId,
        rentalUpdateDTO.StartDate ?? rentalResult.Value.StartDate,
        rentalUpdateDTO.EndDate ?? rentalResult.Value.EndDate,
        rentalUpdateDTO.Id
        );

      if (overlappingReservation.IsFailure)
      {
        _logger.LogWarning("Overlapping reservation detected when updating rental {RentalId} for " +
            "customer {CustomerId}, vehicle {VehicleId}, dates {StartDate:d} to {EndDate:d}: {ErrorMessage}",
            rentalUpdateDTO.Id, rentalResult.Value.CustomerId, rentalResult.Value.VehicleId,
            rentalUpdateDTO.StartDate, rentalUpdateDTO.EndDate, overlappingReservation.Error.Message);
        return Result<bool>.Failure(overlappingReservation.Error);
      }

      var updatedRental = await _rentalRepository.Update(rentalResult.Value);

      return updatedRental.Match(
        rental =>
        {
          _logger.LogInformation("Successfully updated rental {RentalId} for customer {CustomerId}, " +
               "vehicle {VehicleId}, new dates {StartDate:d} to {EndDate:d}",
               rental.ID, rental.CustomerId, rental.VehicleId, rental.StartDate, rental.EndDate);
          return Result<bool>.Success(true);
        },
        error => Result<bool>.Failure(error));
    }

    private async Task<Result<bool>> IsOverlappingReservation(
      int customerId,
      string vehicleId,
      DateTime startDate,
      DateTime endDate,
      int? currentRentalId = null)
    {
      var customerReservationsInTimeFrameResult = await _rentalRepository.GetByCustomerIdInTimeFrame(customerId, startDate, endDate);
      if (customerReservationsInTimeFrameResult.IsFailure)
        return Result<bool>.Failure(customerReservationsInTimeFrameResult.Error);

      var overlappingCustomerReservations = customerReservationsInTimeFrameResult.Value
       .Where(r => currentRentalId == null || r.ID != currentRentalId);

      if (overlappingCustomerReservations.Any())
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this user!"));

      var vehicleReservationsInTimeFrameResult = await _rentalRepository.GetByVinInTimeFrame(vehicleId, startDate, endDate);
      if (vehicleReservationsInTimeFrameResult.IsFailure)
        return Result<bool>.Failure(vehicleReservationsInTimeFrameResult.Error);

      var overlappingVehicleReservations = vehicleReservationsInTimeFrameResult.Value
         .Where(r => currentRentalId == null || r.ID != currentRentalId);

      if (overlappingVehicleReservations.Any())
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this vehicle!"));

      return Result<bool>.Success(true);
    }
  }
}