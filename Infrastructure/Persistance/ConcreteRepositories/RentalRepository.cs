using Core.Domain.Entities;
using Core.Enums;
using Core.Interfaces.Persistence.GenericRepository;
using Core.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Infrastructure.Persistance.GenericRepository;

namespace Infrastructure.Persistance.ConcreteRepositories
{
  internal class RentalRepository : IRentalRepository
  {
    private readonly IRepository<Rental> _rentalRepository;
    public RentalRepository(IRepository<Rental> rentalRpository)
    {
      _rentalRepository = rentalRpository;
    }
    public async Task<Result<Rental>> Create(Rental rental, CancellationToken cancellationToken = default)
    {
      if (rental is null)
        return Result<Rental>.Failure(Error.NullReferenceError("Rental can not be null"));
      try
      {
        var newRental = await _rentalRepository.AddOrUpdateAsync(rental, cancellationToken);
        return Result<Rental>.Success(newRental);
      }
      catch (Exception ex)
      {
        return Result<Rental>.Failure(Error.DatabaseWriteError($"Failed to create rental: {ex.Message}"));
      }
    }

    public async Task<Result<IEnumerable<Rental>>> GetAll(CancellationToken cancellationToken = default)
    {
      try
      {
        var allReservations = await _rentalRepository.GetAsync(cancellationToken);
        return Result<IEnumerable<Rental>>.Success(allReservations);
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Rental>>.Failure(Error.DatabaseReadError($"Failed to retreve Rentals: {ex.Message}"));
      }
    }
    public async Task<Result<Rental>> GetById(int id, CancellationToken cancellationToken = default)
    {
      try
      {
        var rental = await _rentalRepository.GetByIdAsync(id, cancellationToken);
        if (rental is not null)
          return Result<Rental>.Success(rental);
        return Result<Rental>.Failure(Error.NotFound($"Rental with id {id} not found"));
      }
      catch (Exception ex)
      {
        return Result<Rental>.Failure(Error.DatabaseReadError($"Faile to retreve rental with id: {id} {ex.Message}"));
      }
    }

    public async Task<Result<Rental>> Update(Rental rental, CancellationToken cancellationToken = default)
    {
      if (rental is null)
        return Result<Rental>.Failure(Error.NullReferenceError("Rental can not be null"));
      if (rental.ID <= 0)
        return Result<Rental>.Failure(Error.ValidationError("Rental id must be specified for update"));
      try
      {
        var updatedRental = await _rentalRepository.AddOrUpdateAsync(rental, cancellationToken);
        return Result<Rental>.Success(updatedRental);
      }
      catch (Exception ex)
      {
        return Result<Rental>.Failure(Error.DatabaseWriteError($"Unable to update rental with id: {rental.ID}, {ex.Message}"));
      }
    }
    public async Task<Result<IEnumerable<Rental>>> GetByCustomerId(int customerId, CancellationToken cancellationToken = default)
    {
      try
      {
        var specification = new SpecificationBuilder<Rental>(rental => rental.CustomerId == customerId);
        var allRentals = await _rentalRepository.GetAsync(specification, cancellationToken);
        return Result<IEnumerable<Rental>>.Success(allRentals);
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Rental>>.Failure(Error.DatabaseReadError($"Unable to retrieve rentals for customer with id: '{customerId}', {ex.Message}"));
      }
    }

    public async Task<Result<IEnumerable<Rental>>> GetByVin(string vin, CancellationToken cancellationToken = default)
    {
      try
      {
        var specification = new SpecificationBuilder<Rental>(rental => rental.VehicleId == vin);
        var vehicle = await _rentalRepository.GetAsync(specification, cancellationToken);
        if (vehicle is not null)
          return Result<IEnumerable<Rental>>.Success(vehicle);
        return Result<IEnumerable<Rental>>.Failure(Error.NotFound($"Rental with vehicle vin: '{vin}' not found"));
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Rental>>.Failure(Error.DatabaseReadError($"Error occurred while looking for rental with vehicle id: '{vin}', {ex.Message}"));
      }
    }

    public async Task<Result<IEnumerable<Rental>>> GetByCustomerIdInTimeFrame(int customerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
      try
      {
        var specification = new SpecificationBuilder<Rental>(
            rental =>
            rental.CustomerId == customerId &&
            startDate <= rental.EndDate && rental.StartDate <= endDate &&
            rental.RentalStatus != RentalStatus.Cancelled &&
            rental.RentalStatus != RentalStatus.Unknown);

        var allRentals = await _rentalRepository.GetAsync(specification, cancellationToken);
        return Result<IEnumerable<Rental>>.Success(allRentals);
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Rental>>.Failure(Error.DatabaseReadError($"Unable to retrieve rentals for customer with id: '{customerId}', {ex.Message}"));
      }
    }

    public async Task<Result<IEnumerable<Rental>>> GetByVinInTimeFrame(string vin, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
      try
      {
        var specification = new SpecificationBuilder<Rental>(
            rental =>
            rental.VehicleId == vin &&
            startDate <= rental.EndDate && rental.StartDate <= endDate &&
            (rental.RentalStatus != RentalStatus.Cancelled && rental.RentalStatus != RentalStatus.Unknown));

        var vehicle = await _rentalRepository.GetAsync(specification, cancellationToken);

        if (vehicle is not null)
          return Result<IEnumerable<Rental>>.Success(vehicle);
        return Result<IEnumerable<Rental>>.Failure(Error.NotFound($"Rental with vehicle vin: {vin} not found"));
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Rental>>.Failure(Error.DatabaseReadError($"Error occurred while looking for rental with vehicle id: {vin}, {ex.Message}"));
      }
    }
  }
}
