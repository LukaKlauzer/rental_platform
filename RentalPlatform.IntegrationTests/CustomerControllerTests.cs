using System.Net.Http.Json;
using Core.DTOs.Customer;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using rental_platform.Controllers;

namespace RentalPlatform.IntegrationTests
{
  public class CustomerControllerTests :
    IClassFixture<WebApplicationFactory<CustomerController>>,
    IAsyncLifetime
  {
    private readonly WebApplicationFactory<CustomerController> _webApplicationFactory;

    private List<int> _customerIds;
    public CustomerControllerTests(WebApplicationFactory<CustomerController> webApplicationFactory)
    {
      _webApplicationFactory = webApplicationFactory;

      _customerIds = new List<int>();
    }



    [Fact]
    public async Task GivenValidCustomerDto_CreatesCustomer()
    {
      // Arange
      var httpClient = _webApplicationFactory.CreateClient();
      var customerCreateDto = new CustomerCreateDTO() { Name = "Test user 1" };

      // Act
      var response = await httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
      var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerReturnDTO>();

      // Assert
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
      createdCustomer.Should().NotBeNull();
      createdCustomer.Name.Should().Be(customerCreateDto.Name);

      _customerIds.Add(createdCustomer.Id);
    }

    [Fact]
    public async Task CreateCustomer_RetrieveCustomerById() 
    {
      // Arange
      var httpClient = _webApplicationFactory.CreateClient();
      var customerCreateDto = new CustomerCreateDTO() { Name = "Test user 2"};

      // Act
      var responce = await httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
      var createdCustomer = await responce.Content.ReadFromJsonAsync<CustomerReturnDTO>();
      createdCustomer.Should().NotBeNull();

      // Assert
      var getCustomer = await httpClient.GetAsync($"api/customer/{createdCustomer.Id}");
      getCustomer.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

      var retreaveCustomer = await getCustomer.Content.ReadFromJsonAsync<CustomerReturnDTO>();
      retreaveCustomer.Should().NotBeNull();
      retreaveCustomer.Name.Should().Be(customerCreateDto.Name);
      
      _customerIds.Add(createdCustomer.Id);
    }


    public Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
      var httpClient = _webApplicationFactory?.CreateClient();
      foreach (var id in _customerIds)
        await httpClient.DeleteAsync($"api/customer/{id}");

    }
  }
}
