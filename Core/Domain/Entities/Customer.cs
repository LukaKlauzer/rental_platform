using Core.Domain.Common;
using Core.Domain.EntityValidation;
using Core.Result;

namespace Core.Domain.Entities
{
  public class Customer : EntityID
  {
    private Customer() { }
    private Customer(string name)
    {
      Name = name;
      IsDeleted = false;
    }

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

    public string Name { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    public ICollection<Rental> RentalRecords { get; private set; } = new List<Rental>();

  }
}