using Application.DTOs.Customer;
using Application.Interfaces.Mapers;
using Core.Domain.Entities;
using Core.Result;

namespace Application.Mapers
{
  internal class CustomerMapper : ICustomerMapper
    {
      public Result<Customer> ToEntity(CustomerCreateDto dto)
      {
        if (dto is null)
          return Result<Customer>.Failure(Error.NullReferenceError("CustomerCreateDto can not be null"));

        return Customer.Create(dto.Name);
      }
    public Result<CustomerReturnDto> ToReturnDto(Customer entity)
      {
        if (entity is null)
          return Result<CustomerReturnDto>.Failure(Error.NullReferenceError("Customer entity cannot be null"));

        var dto = new CustomerReturnDto(
          Id: entity.ID,
          Name: entity.Name,
          IsDeleted: entity.IsDeleted
        );

        return Result<CustomerReturnDto>.Success(dto);
      }

      public Result<CustomerReturnSingleDto> ToReturnSingleDto(Customer entity, float totalDistanceDriven = 0f, float totalPrice = 0f)
      {
        if (entity is null)
          return Result<CustomerReturnSingleDto>.Failure(Error.NullReferenceError("Customer entity cannot be null"));

        var dto = new CustomerReturnSingleDto(
          Id: entity.ID,
          Name: entity.Name,
          IsDeleted: entity.IsDeleted,
          TotalDistanceDriven: totalDistanceDriven,
          TotalPrice: totalPrice
        );

        return Result<CustomerReturnSingleDto>.Success(dto);
      }

      public Result<List<CustomerReturnDto>> ToReturnDtoList(IEnumerable<Customer> entities)
      {
        if (entities is null)
          return Result<List<CustomerReturnDto>>.Failure(Error.NullReferenceError("Customer collection cannot be null"));

        var dtos = new List<CustomerReturnDto>();

        foreach (var entity in entities)
        {
          var dtoResult = ToReturnDto(entity);
          if (dtoResult.IsFailure)
            return Result<List<CustomerReturnDto>>.Failure(dtoResult.Error);

          dtos.Add(dtoResult.Value);
        }

        return Result<List<CustomerReturnDto>>.Success(dtos);
      }
  }
}
