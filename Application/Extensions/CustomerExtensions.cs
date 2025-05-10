using Core.Domain.Entities;
using Application.DTOs.Customer;

namespace Application.Extensions
{
  public static class CustomerExtensions
  {
    public static Customer? ToCustomer(this CustomerCreateDto customerCreateDTO)
    {
      if (customerCreateDTO is null) return null;

      return new Customer()
      {
        
        Name = customerCreateDTO.Name
      };
    }
    public static Customer? ToCustomer(this CustomerUpdateDto customerUpdateDTO)
    {
      if (customerUpdateDTO is null) return null;

      return new Customer()
      {
        ID = customerUpdateDTO.Id,
        Name = customerUpdateDTO.Name
      };
    }
    public static CustomerReturnDto? ToReturnDto(this Customer customer)
    {
      if (customer is null) return null;

      return new CustomerReturnDto()
      {
        Id = customer.ID,
        Name = customer.Name,
        IsDeleted = customer.IsDeleted
      };
    }
    public static CustomerReturnSingleDto? ToReturnSingleDto(this Customer customer)
    {
      if (customer is null) return null;

      return new CustomerReturnSingleDto()
      {
        Id = customer.ID,
        Name = customer.Name,
        IsDeleted = customer.IsDeleted
      };
    }

    public static List<CustomerReturnDto> ToListReturnDto(this List<Customer> customers)
    {
      if (customers is null) return new List<CustomerReturnDto>();
      return customers.Select(customer => new CustomerReturnDto()
      {
        Id = customer.ID,
        Name = customer.Name,
        IsDeleted = customer.IsDeleted
      }).ToList();
    }

  }
}
