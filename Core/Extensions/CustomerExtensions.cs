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
    public static CustomerReturnDTO? ToReturnDto(this Customer customer)
    {
      if (customer is null) return null;

      return new CustomerReturnDTO()
      {
        Name = customer.Name
      };
    }
    public static CustomerReturnSingleDTO? ToReturnSingleDto(this Customer customer)
    {
      if (customer is null) return null;

      return new CustomerReturnSingleDTO()
      {
        Name = customer.Name
      };
    }

    public static List<CustomerReturnDTO> ToListReturnDto(this List<Customer> customers)
    {
      if (customers is null) return new List<CustomerReturnDTO>();
     return customers.Select(customer => new CustomerReturnDTO() { Name = customer.Name }).ToList();
    }
    
  }
}
