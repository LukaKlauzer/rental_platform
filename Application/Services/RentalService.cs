using Core.DTOs.Rental;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Interfaces.Services;
using Core.Result;

namespace Application.Services
{
  internal class RentalService : IRentalService
  {
    private IRentalRepository _rentalRepository;
    private ICustomerRepository _customerRepository;
    private IVehicleRepository _vehicleRepository;
    public RentalService(
      IRentalRepository rentalRepository,
      ICustomerRepository customerRepository,
      IVehicleRepository vehicleRepository)
    {
      _rentalRepository = rentalRepository;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
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

      // TODO Take snapshots from telemetry

      var rentalCreate = rentalCreateDTO.ToRental();
      if (rentalCreate is null)
        return Result<RentalReturnDTO>.Failure(Error.MappingError("Failed to map reservation DTO to reservation entity"));

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

      var rentalResult = await _rentalRepository.GetById(rentalUpdateDTO.Id);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      var overlappingReservation = await IsOverlappingReservation(
        rentalResult.Value.CustomerId,
        rentalResult.Value.VehicleId,
        rentalUpdateDTO.StartDate,
        rentalUpdateDTO.EndDate);

      if (overlappingReservation.IsFailure)
        return Result<bool>.Failure(overlappingReservation.Error);

      var rentalToupdate = rentalUpdateDTO.ToRental();
      if (rentalToupdate is null)
        return Result<bool>.Failure(Error.MappingError("Falure while maping rental update DTO torental entity"));
      
      var updatedRental = await _rentalRepository.Update(rentalToupdate);

      return updatedRental.Match(
        rental=>Result<bool>.Success(true),
        error=> Result<bool>.Failure(error));
    }

    private async Task<Result<bool>> IsOverlappingReservation(int customerId, string vehicleId, DateTime startDate, DateTime endDate)
    {
      var customerReservationsInTimeFrameResul = await _rentalRepository.GetByCustomerIdInTimeFrame(customerId, startDate, endDate);
      if (customerReservationsInTimeFrameResul.IsFailure)
        return Result<bool>.Failure(customerReservationsInTimeFrameResul.Error);

      if (customerReservationsInTimeFrameResul.Value.Any())
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this user!"));

        var vehicleReservationsInTimeFrameResul = await _rentalRepository.GetByVinInTimeFrame(vehicleId, startDate, endDate);
      if (vehicleReservationsInTimeFrameResul.IsFailure)
        return Result<bool>.Failure(vehicleReservationsInTimeFrameResul.Error);

      if(vehicleReservationsInTimeFrameResul.Value.Any())
        return Result<bool>.Failure(Error.ValidationError("Requested reservation overlaps for this vehicle!"));

      return Result<bool>.Success(true);
    }
  }
}