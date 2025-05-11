using Core.Domain.Entities;
using Application.Interfaces.Persistence.GenericRepository;
using Application.Interfaces.Persistence.SpecificRepository;
using Core.Result;
using Infrastructure.Persistance.GenericRepository;

namespace Infrastructure.Persistance.ConcreteRepositories
{
  internal class VehicleRepository : IVehicleRepository
  {
    private readonly IRepository<Vehicle> _vehicleRepository;
    public VehicleRepository(IRepository<Vehicle> vehicleRepository)
    {
      _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<Vehicle>> Create(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
      if (vehicle is null)
        return Result<Vehicle>.Failure(Error.NullReferenceError("Vehicle cannot be null"));

      try
      {
        var newVehicle = await _vehicleRepository.AddOrUpdateAsync(vehicle, cancellationToken);
        return Result<Vehicle>.Success(newVehicle);
      }
      catch (Exception ex)
      {
        return Result<Vehicle>.Failure(Error.DatabaseWriteError($"Failed to create vehicle: {ex.Message}"));
      }
    }

    public async Task<Result<List<Vehicle>>> CreateBulk(List<Vehicle> vehicles, CancellationToken cancellationToken = default)
    {
      if (vehicles is null || !vehicles.Any())
        return Result<List<Vehicle>>.Success(new List<Vehicle>());

      try
      {
        await _vehicleRepository.BeginTransactionAsync(cancellationToken);
        var savedVehicles = await _vehicleRepository.AddAsync(vehicles, cancellationToken);
        await _vehicleRepository.CommitTransactionAsync(cancellationToken);

        return Result<List<Vehicle>>.Success(savedVehicles.ToList());
      }
      catch (Exception ex)
      {
        await _vehicleRepository.RollbackTransactionAsync(cancellationToken);
        return Result<List<Vehicle>>.Failure(Error.DatabaseWriteError($"Failed to create vehicle records in bulk: {ex.Message}"));
      }
    }

    public async Task<Result<List<Vehicle>>> GetAll(CancellationToken cancellationToken = default)
    {
      try
      {
        var vehicles = await _vehicleRepository.GetAsync(cancellationToken);
        return Result<List<Vehicle>>.Success(vehicles.ToList());
      }
      catch (Exception ex)
      {
        return Result<List<Vehicle>>.Failure(Error.DatabaseReadError($"Failed to retrieve vehicles: {ex.Message}"));
      }

    }

    public async Task<Result<Vehicle>> GetById(string vin, CancellationToken cancellationToken = default)
    {
      try
      {
        var specification = new SpecificationBuilder<Vehicle>(vehicle => vehicle.Vin == vin);
        var vehicle = await _vehicleRepository.GetFirstAsync(specification, cancellationToken);

        if (vehicle is not null)
          return Result<Vehicle>.Success(vehicle);

        return Result<Vehicle>.Failure(Error.NotFound($"Vehicle with id {vin} not found"));
      }
      catch (Exception ex)
      {
        return Result<Vehicle>.Failure(Error.DatabaseReadError($"Error retrieving vehicle with vin {vin}: {ex.Message}"));
      }
    }

    public async Task<Result<Vehicle>> GetByVin(string vin, CancellationToken cancellationToken = default)
    {
      try
      {
        var specification = new SpecificationBuilder<Vehicle>(vehicle => vehicle.Vin == vin);
        var vehicle = await _vehicleRepository.GetFirstAsync(specification, cancellationToken);
        if (vehicle is not null)
          return Result<Vehicle>.Success(vehicle);
        return Result<Vehicle>.Failure(Error.NotFound($"Vehicle with vin: {vin} not found"));
      }
      catch (Exception ex)
      {
        return Result<Vehicle>.Failure(Error.DatabaseReadError($"Error retrieving vehicle with vin {vin}, {ex.Message}"));
      }
    }

    public async Task<Result<IEnumerable<Vehicle>>> GetByVins(List<string> vins, CancellationToken cancellationToken = default)
    {
      try
      {
        var specification = new SpecificationBuilder<Vehicle>(vehicle => vins.Contains(vehicle.Vin));
        var vehicles = await _vehicleRepository.GetAsync(specification, cancellationToken);
        return Result<IEnumerable<Vehicle>>.Success(vehicles);
      }
      catch (Exception ex)
      {
        return Result<IEnumerable<Vehicle>>.Failure(Error.DatabaseReadError($"Error retrieving vehicles: {ex.Message}"));
      }
    }
  }
}