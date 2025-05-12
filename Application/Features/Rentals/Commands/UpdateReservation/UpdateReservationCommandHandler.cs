using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Rentals.Commands.UpdateReservation
{
  public class UpdateReservationCommandHandler : IRequestHandler<UpdateReservationCommand, Result<bool>>
  {
    private readonly ILogger<UpdateReservationCommandHandler> _logger;
    private readonly IRentalRepository _rentalRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public UpdateReservationCommandHandler(
        ILogger<UpdateReservationCommandHandler> logger,
        IRentalRepository rentalRepository,
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository)
    {
      _logger = logger;
      _rentalRepository = rentalRepository;
      _customerRepository = customerRepository;
      _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<bool>> Handle(UpdateReservationCommand command, CancellationToken cancellationToken)
    {
      var rentalResult = await _rentalRepository.GetByIdWithCustomerAndVehicle(command.Id, cancellationToken);
      if (rentalResult.IsFailure)
        return Result<bool>.Failure(rentalResult.Error);

      var customerResult = await _customerRepository.GetByIdWithRentals(
          rentalResult.Value.CustomerId, cancellationToken);
      if (customerResult.IsFailure)
        return Result<bool>.Failure(customerResult.Error);

      var vehicleResult = await _vehicleRepository.GetByVinWithRentals(
          rentalResult.Value.VehicleId, cancellationToken);
      if (vehicleResult.IsFailure)
        return Result<bool>.Failure(vehicleResult.Error);

      var updateResult = rentalResult.Value.UpdateDates(
          command.StartDate,
          command.EndDate,
          customerResult.Value,
          vehicleResult.Value);

      if (updateResult.IsFailure)
        return updateResult;

      var savedResult = await _rentalRepository.Update(rentalResult.Value, cancellationToken);
      return savedResult.Match(
          rental => Result<bool>.Success(true),
          error => Result<bool>.Failure(error));
    }
  }
}
