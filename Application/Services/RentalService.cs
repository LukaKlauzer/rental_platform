using Core.DTOs.Rental;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Services;
using Core.Result;

namespace Application.Services
{
  public class RentalService : IRentalService
  {
    private readonly IRentalRepository _rentalRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITelemetryRepository _telemetryRepository;
    public RentalService(
      IRentalRepository rentalRepository,
      ICustomerRepository customerRepository,
      IVehicleRepository vehicleRepository,
      ITelemetryRepository telemetryRepository
      )
    {
      _rentalRepository = rentalRepository;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
      _telemetryRepository = telemetryRepository;
    }

    public async Task<Result<RentalReturnDTO>> CreateReservation(RentalCreateDTO rentalCreateDTO)
    {
      if (rentalCreateDTO.CustomerId <= 0)
        return Result<RentalReturnDTO>.Failure(Error.ValidationError($"Customer id is not valid: {rentalCreateDTO.CustomerId}"));

      if (string.IsNullOrEmpty(rentalCreateDTO.VehicleId))
        return Result<RentalReturnDTO>.Failure(Error.ValidationError($"Vehicle vin is not valid: {rentalCreateDTO.VehicleId}"));

      // Get customer
      var customerResult = await _customerRepository.GetById(rentalCreateDTO.CustomerId);
      if (customerResult.IsFailure)
        return Result<RentalReturnDTO>.Failure(customerResult.Error);
      if (customerResult.Value.IsDeleted == true)
        return Result<RentalReturnDTO>.Failure(Error.ValidationError("Customer is no longer active or has been deleted."));

      // Get vehicle
      var vehicleResult = await _vehicleRepository.GetByVin(rentalCreateDTO.VehicleId);
      if (vehicleResult.IsFailure)
        return Result<RentalReturnDTO>.Failure(vehicleResult.Error);

      var overlappingReservation = await IsOverlappingReservation(
        rentalCreateDTO.CustomerId,
        rentalCreateDTO.VehicleId,
        rentalCreateDTO.StartDate,
        rentalCreateDTO.EndDate);
      if (overlappingReservation.IsFailure)
        return Result<RentalReturnDTO>.Failure(overlappingReservation.Error);

      var odometerStartResult = await _telemetryRepository.GetMostRecentBefore(rentalCreateDTO.VehicleId, rentalCreateDTO.StartDate, TelemetryType.odometer);
      if (odometerStartResult.IsFailure)
        return Result<RentalReturnDTO>.Failure(odometerStartResult.Error);

      var odometerEndResult = await _telemetryRepository.GetEarliestAfter(rentalCreateDTO.VehicleId, rentalCreateDTO.EndDate, TelemetryType.odometer);

      var batterySocStartResult = await _telemetryRepository.GetMostRecentBefore(rentalCreateDTO.VehicleId, rentalCreateDTO.StartDate, TelemetryType.battery_soc);
      if (batterySocStartResult.IsFailure)
        return Result<RentalReturnDTO>.Failure(batterySocStartResult.Error);

      var batterySocEndResult = await _telemetryRepository.GetEarliestAfter(rentalCreateDTO.VehicleId, rentalCreateDTO.EndDate, TelemetryType.battery_soc);

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

      var newReservation = await _rentalRepository.Create(rentalCreate);
      return newReservation.Match(
        reservation =>
        {
          var returnDto = reservation.ToReturnDto();
          if (returnDto is not null)
            return Result<RentalReturnDTO>.Success(returnDto);
          return Result<RentalReturnDTO>.Failure(Error.MappingError("Failed to map reservation entity to reservation DTO"));
        },
        error => Result<RentalReturnDTO>.Failure(error)
        );
    }
    public async Task<Result<bool>> CancelReservation(int id)
    {
      var rentalResult = await _rentalRepository.GetById(id);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      rentalResult.Value.RentalStatus = RentalStatus.Cancelled;

      var updatedRentalResult = await _rentalRepository.Update(rentalResult.Value);

      return updatedRentalResult.Match(
        rental => Result<bool>.Success(true),
        error => Result<bool>.Failure(error)
        );
    }

    public async Task<Result<List<RentalReturnDTO>>> GetAll()
    {
      var allRentalsResult = await _rentalRepository.GetAll();

      return allRentalsResult.Match(
        rentals => Result<List<RentalReturnDTO>>.Success(rentals.ToList().ToListRentalDto()),
        error => Result<List<RentalReturnDTO>>.Failure(error)
        );
    }

    public async Task<Result<RentalReturnSingleDTO>> GetById(int id)
    {
      var rentalResult = await _rentalRepository.GetById(id);
      if (rentalResult.IsFailure)
        return Result<RentalReturnSingleDTO>.Failure(rentalResult.Error);

      var returnDto = rentalResult.Value.ToReturnSingleDto();
      if (returnDto is null)
        return Result<RentalReturnSingleDTO>.Failure(Error.MappingError("Faile to map rental entity to rental DTO"));

      if (rentalResult.Value.OdometerEnd.HasValue)
        returnDto.DistanceTraveled = rentalResult.Value.OdometerEnd.Value - rentalResult.Value.OdometerStart;

      returnDto.BatterySOCSAtStart = rentalResult.Value.BatterySOCStart;

      if (rentalResult.Value.BatterySOCEnd.HasValue)
        returnDto.BatterySOCAtEnd = rentalResult.Value.BatterySOCEnd.Value;

      return Result<RentalReturnSingleDTO>.Success(returnDto);
    }

    public async Task<Result<bool>> UpdateReservation(RentalUpdateDTO rentalUpdateDTO)
    {
      if (rentalUpdateDTO.Id <= 0)
        return Result<bool>.Failure(Error.ValidationError($"Rental id is not valid: {rentalUpdateDTO.Id}"));
      if(rentalUpdateDTO.StartDate is null && rentalUpdateDTO.EndDate is null)
        return Result<bool>.Failure(Error.ValidationError("At least one field must be provided for update"));

      var rentalResult = await _rentalRepository.GetById(rentalUpdateDTO.Id);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      if (rentalUpdateDTO.StartDate.HasValue)
        rentalResult.Value.StartDate = rentalUpdateDTO.StartDate.Value;
      if (rentalUpdateDTO.EndDate.HasValue)
        rentalResult.Value.EndDate = rentalUpdateDTO.EndDate.Value;

      var overlappingReservation = await IsOverlappingReservation(
        rentalResult.Value.CustomerId,
        rentalResult.Value.VehicleId,
        rentalUpdateDTO.StartDate??rentalResult.Value.StartDate,
        rentalUpdateDTO.EndDate??rentalResult.Value.EndDate,
        rentalUpdateDTO.Id
        );

      if (overlappingReservation.IsFailure)
        return Result<bool>.Failure(overlappingReservation.Error);


      if (rentalUpdateDTO.StartDate.HasValue)
        rentalResult.Value.StartDate = rentalUpdateDTO.StartDate.Value;
      if (rentalUpdateDTO.EndDate.HasValue)
        rentalResult.Value.EndDate = rentalUpdateDTO.EndDate.Value;
      
      var updatedRental = await _rentalRepository.Update(rentalResult.Value);

      return updatedRental.Match(
        rental => Result<bool>.Success(true),
        error => Result<bool>.Failure(error));
    }

    private async Task<Result<bool>> IsOverlappingReservation(int customerId, string vehicleId, DateTime startDate, DateTime endDate, int? currentRentalId = null)
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