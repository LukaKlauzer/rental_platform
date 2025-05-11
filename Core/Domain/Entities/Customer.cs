using Core.Domain.Common;
using Core.Domain.EntityValidation;
using Core.Enums;
using Core.Result;
using static Core.Domain.Entities.Rental;

namespace Core.Domain.Entities
{
  public class Customer : EntityID
  {
    public string Name { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    private readonly List<Rental> _rentals = new();
    public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();
    private Customer() { }
    private Customer(string name)
    {
      Name = name;
      IsDeleted = false;
    }

    public bool HasRentals() => _rentals.Any();
    public static Result<Customer> Create(string name)
    {
      var validationResult = CustomerValidator.ValidateCustomerData(name);

      if (validationResult.IsFailure)
        return Result<Customer>.Failure(validationResult.Error);

      var customer = new Customer(name);

      return Result<Customer>.Success(customer);
    }
    public void MarkAsDeleted()
    {
      IsDeleted = true;
    }
    public Result<bool> Update(string name)
    {
      var validationResult = CustomerValidator.ValidateCustomerData(name);

      if (validationResult.IsFailure)
        return Result<bool>.Failure(validationResult.Error);

      Name = name;

      return Result<bool>.Success(true);
    }
    public bool CanRentVehicle() => !IsDeleted;
    public bool HasOverlappingRental(DateTime startDate, DateTime endDate)
    {
      return _rentals.Any(r =>
          r.RentalStatus == RentalStatus.Ordered &&
          r.OverlapsWith(startDate, endDate));
    }
    public Result<bool> CanCreateRental(DateTime startDate, DateTime endDate)
    {
      if (IsDeleted)
        return Result<bool>.Failure(Error.ValidationError("Customer is deleted and cannot rent"));

      if (HasOverlappingRental(startDate, endDate))
        return Result<bool>.Failure(Error.ValidationError("Customer already has a rental in this time period"));

      return Result<bool>.Success(true);
    }
    public Result<CustomerStatistics> CalculateStatistics(IEnumerable<Vehicle> vehicles)
    {
      var completedRentals = _rentals
          .Where(r => r.IsCompleted() && !r.IsCancelled())
          .ToList();

      if (!completedRentals.Any())
        return Result<CustomerStatistics>.Success(new CustomerStatistics(0, 0));

      var vehicleDict = vehicles.ToDictionary(v => v.Vin);

      var results = new List<RentalCost>();
      foreach (var rental in completedRentals)
      {
        if (!vehicleDict.TryGetValue(rental.VehicleId, out var vehicle))
          continue; 

        var costResult = rental.CalculateCost(vehicle);
        if (costResult.IsSuccess)
          results.Add(costResult.Value);
      }

      if (!results.Any())
        return Result<CustomerStatistics>.Success(new CustomerStatistics(0, 0));
      var totalDistance = results.Sum(s => s.Distance);
      var totalCost = results.Sum(s => s.TotalCost);

      return Result<CustomerStatistics>.Success(new CustomerStatistics(totalDistance, totalCost)); ;
    }
    public record CustomerStatistics(float TotalDistanceDriven, float TotalPrice);
  }
}