using Core.Domain.Entities;
using Core.DTOs.Customer;

namespace Core.Extensions
{
  public static class CustomerExtensions
  {
    public static Customer? ToCustomer(this CustomerCreateDTO customerCreateDTO)
    {
      if (customerCreateDTO is null) return null;

      return new Customer()
      {
        
        Name = customerCreateDTO.Name
      };
    }
    public static Customer? ToCustomer(this CustomerUpdateDTO customerUpdateDTO)
    {
      if (customerUpdateDTO is null) return null;

      return new Customer()
      {
        ID = customerUpdateDTO.Id,
        Name = customerUpdateDTO.Name
      };
    }
    public static CustomerReturnDTO? ToReturnDto(this Customer customer)
    {
      if (customer is null) return null;

      return new CustomerReturnDTO()
      {
        Id = customer.ID,
        Name = customer.Name,
        IsDeleted = customer.IsDeleted
      };
    }
    public static CustomerReturnSingleDTO? ToReturnSingleDto(this Customer customer)
    {
      if (customer is null) return null;

      return new CustomerReturnSingleDTO()
      {
        Id = customer.ID,
        Name = customer.Name,
        IsDeleted = customer.IsDeleted
      };
    }

    public static List<CustomerReturnDTO> ToListReturnDto(this List<Customer> customers)
    {
      if (customers is null) return new List<CustomerReturnDTO>();
      return customers.Select(customer => new CustomerReturnDTO()
      {
        Id = customer.ID,
        Name = customer.Name,
        IsDeleted = customer.IsDeleted
      }).ToList();
    }

  }
}
