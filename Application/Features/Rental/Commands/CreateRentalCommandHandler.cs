using Core.DTOs.Rental;
using Core.Enums;
using Core.Features.Rental.Commands;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Core.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Rental.Commands
{
  public class CreateRentalCommandHandler : IRequestHandler<CreateRentalCommand, Result<RentalReturnDTO>>
  {
    private readonly ILogger<CreateRentalCommandHandler> _logger;
    private readonly IRentalRepository _rentalRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITelemetryRepository _telemetryRepository;

    public CreateRentalCommandHandler(
        ILogger<CreateRentalCommandHandler> logger,
        IRentalRepository rentalRepository,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        ITelemetryRepository telemetryRepository)
    {
      _logger = logger;
      _rentalRepository = rentalRepository;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
      _telemetryRepository = telemetryRepository;
    }

    public async Task<Result<RentalReturnDTO>> Handle(CreateRentalCommand request, CancellationToken cancellationToken)
    {
      if (request.CustomerId <= 0)
        return Result<RentalReturnDTO>.Failure(Error.ValidationError($"Customer id is not valid: {request.CustomerId}"));

      if (string.IsNullOrEmpty(request.VehicleId))
        return Result<RentalReturnDTO>.Failure(Error.ValidationError($"Vehicle vin is not valid: {request.VehicleId}"));

      // Get customer
      var customerResult = await _customerRepository.GetById(request.CustomerId, cancellationToken);
      if (customerResult.IsFailure)
      {
        _logger.LogWarning("Failed to retrieve customer {CustomerId} for rental creation: {ErrorMessage}",
          request.CustomerId, customerResult.Error.Message);
        return Result<RentalReturnDTO>.Failure(customerResult.Error);
      }
      if (customerResult.Value.IsDeleted == true)
        return Result<RentalReturnDTO>.Failure(Error.ValidationError("Customer is no longer active or has been deleted."));

      // Get vehicle
      var vehicleResult = await _vehicleRepository.GetByVin(request.VehicleId, cancellationToken);
      if (vehicleResult.IsFailure)
      {
        _logger.LogWarning("Failed to retrieve vehicle {VehicleId} for rental creation: {ErrorMessage}",
          request.VehicleId, customerResult.Error.Message);
        return Result<RentalReturnDTO>.Failure(vehicleResult.Error);
      }

      var overlappingReservationResult = await IsOverlappingReservation(
          request.CustomerId,
          request.VehicleId,
          request.StartDate,
          request.EndDate,
          cancellationToken);

      if (overlappingReservationResult.IsFailure)
      {
        _logger.LogWarning("Overlapping reservation detected for customer {CustomerId}, vehicle {VehicleId}, dates {StartDate:d} to {EndDate:d}: {ErrorMessage}",
            request.CustomerId, request.VehicleId, request.StartDate, request.EndDate,
            overlappingReservationResult.Error.Message);
        return Result<RentalReturnDTO>.Failure(overlappingReservationResult.Error);
      }

      var odometerStartResult = await _telemetryRepository.GetMostRecentBefore(
          request.VehicleId, request.StartDate, TelemetryType.odometer, cancellationToken);

      if (odometerStartResult.IsFailure)
        return Result<RentalReturnDTO>.Failure(odometerStartResult.Error);

      var odometerEndResult = await _telemetryRepository.GetEarliestAfter(
          request.VehicleId, request.EndDate, TelemetryType.odometer, cancellationToken);

      var batterySocStartResult = await _telemetryRepository.GetMostRecentBefore(
          request.VehicleId, request.StartDate, TelemetryType.battery_soc, cancellationToken);

      if (batterySocStartResult.IsFailure)
        return Result<RentalReturnDTO>.Failure(batterySocStartResult.Error);

      var batterySocEndResult = await _telemetryRepository.GetEarliestAfter(
          request.VehicleId, request.EndDate, TelemetryType.battery_soc, cancellationToken);

      var rentalCreateDTO = new RentalCreateDTO
      {
        CustomerId = request.CustomerId,
        VehicleId = request.VehicleId,
        StartDate = request.StartDate,
        EndDate = request.EndDate
      };

      var rentalCreate = rentalCreateDTO.ToRental();
      if (rentalCreate is null)
        return Result<RentalReturnDTO>.Failure(Error.MappingError("Failed to map reservation DTO to reservation entity"));

      // Set the mandatory start values
      rentalCreate.OdometerStart = odometerStartResult.Value.Value;
      rentalCreate.BatterySOCStart = batterySocStartResult.Value.Value;

      // Set end values if available
      if (odometerEndResult.IsSuccess)
        rentalCreate.OdometerEnd = odometerEndResult.Value.Value;

      if (batterySocEndResult.IsSuccess)
        rentalCreate.BatterySOCEnd = batterySocEndResult.Value.Value;

      var newReservation = await _rentalRepository.Create(rentalCreate, cancellationToken);

      return newReservation.Match(
          reservation =>
          {
            var returnDto = reservation.ToReturnDto();
            if (returnDto is not null)
            {
              _logger.LogInformation("Successfully created rental reservation with ID: {RentalId} for customer {CustomerId}, vehicle {VehicleId}, from {StartDate:d} to {EndDate:d}",
                      reservation.ID, reservation.CustomerId, reservation.VehicleId, reservation.StartDate, reservation.EndDate);
              return Result<RentalReturnDTO>.Success(returnDto);
            }
            return Result<RentalReturnDTO>.Failure(Error.MappingError("Failed to map reservation entity to reservation DTO"));
          },
          error =>
          {
            _logger.LogError("Failed to create rental reservation: {ErrorMessage}", error.Message);
            return Result<RentalReturnDTO>.Failure(error);
          }
      );
    }
    private async Task<Result<bool>> IsOverlappingReservation(
            int customerId,
            string vehicleId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken,
            int? currentRentalId = null)
    {
      var customerReservationsInTimeFrameResult = await _rentalRepository.GetByCustomerIdInTimeFrame(
          customerId, startDate, endDate, cancellationToken);

      if (customerReservationsInTimeFrameResult.IsFailure)
        return Result<bool>.Failure(customerReservationsInTimeFrameResult.Error);

      var overlappingCustomerReservations = customerReservationsInTimeFrameResult.Value
         .Where(r => currentRentalId == null || r.ID != currentRentalId);

      if (overlappingCustomerReservations.Any())
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this user!"));

      var vehicleReservationsInTimeFrameResult = await _rentalRepository.GetByVinInTimeFrame(
          vehicleId, startDate, endDate, cancellationToken);

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
