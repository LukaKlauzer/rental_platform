using System.Net.Http.Json;
using Core.DTOs.Customer;
using FluentAssertions;
using RentalPlatform.Tests.Integration;

namespace RentalPlatform.IntegrationTests
{
  public class CustomerControllerTests :
    IClassFixture<CustomWebApplicationFactory<Program>>,
    IAsyncLifetime
  {
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private List<int> _customerIds;
    public CustomerControllerTests(CustomWebApplicationFactory<Program> factory)
    {

      _factory = factory; // uses in memory by defoult set to: = new TestWebApplicationFactory<Program>(useInMemoryDatabase: false); in oder to use real db
      _httpClient = _factory.CreateClient();
      _customerIds = new List<int>();
    }


    [Fact]
    public async Task GivenValidCustomerDto_CreatesCustomer()
    {
  
      // Arange
      
      var customerCreateDto = new CustomerCreateDTO() { Name = "Test user 1" };

      // Act
      var response = await _httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
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
     
      var customerCreateDto = new CustomerCreateDTO() { Name = "Test user 2" };

      // Act
      var responce = await _httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
      var createdCustomer = await responce.Content.ReadFromJsonAsync<CustomerReturnDTO>();
      createdCustomer.Should().NotBeNull();

      // Assert
      var getCustomer = await _httpClient.GetAsync($"api/customer/{createdCustomer.Id}");
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
       await Task.CompletedTask;
      //var httpClient = _factory?.CreateClient();
      //foreach (var id in _customerIds)
      //  await httpClient.DeleteAsync($"api/customer/{id}");

    }
  }
}
