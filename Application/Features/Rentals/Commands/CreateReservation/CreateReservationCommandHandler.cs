using Application.DTOs.Rental;
using Application.Interfaces.Mapers;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Enums;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Rentals.Commands.CreateReservation
{
  public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, Result<RentalReturnDto>>
  {
    private readonly ILogger<CreateReservationCommandHandler> _logger;
    private readonly IRentalRepository _rentalRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IRentalMapper _rentalMapper;

    public CreateReservationCommandHandler(
        ILogger<CreateReservationCommandHandler> logger,
        IRentalRepository rentalRepository,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        IRentalMapper rentalMapper)
    {
      _logger = logger;
      _rentalRepository = rentalRepository;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
      _rentalMapper = rentalMapper;
    }

    public async Task<Result<RentalReturnDto>> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
    {
      var dto = new RentalCreateDto(command.StartDate, command.EndDate, command.VehicleId, command.CustomerId);

      var customerResult = await _customerRepository.GetByIdWithRentals(dto.CustomerId, cancellationToken);
      if (customerResult.IsFailure)
        return Result<RentalReturnDto>.Failure(customerResult.Error);

      var canCreateRentalResult = customerResult.Value.CanCreateRental(dto.StartDate, dto.EndDate);
      if (canCreateRentalResult.IsFailure)
        return Result<RentalReturnDto>.Failure(canCreateRentalResult.Error);

      var vehicleResult = await _vehicleRepository.GetByVinWithTelemetryAndRentals(dto.VehicleId, cancellationToken);
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

      var rentalResult = Core.Domain.Entities.Rental.Create(
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

      var savedRentalResult = await _rentalRepository.Create(rentalResult.Value, cancellationToken);
      if (savedRentalResult.IsFailure)
        return Result<RentalReturnDto>.Failure(savedRentalResult.Error);

      return _rentalMapper.ToReturnDto(savedRentalResult.Value);
    }
  }
}