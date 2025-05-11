using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs.Customer;
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
    public async Task CreateCustomer_ShouldReturnSuccess()
    {
      // Arrange
      var customerCreateDto = new CustomerCreateDto("Test Customer 1");

      // Act - Create endpoint doesn't require auth
      var response = await _httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
      var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerReturnDto>();

      // Assert
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
      createdCustomer.Should().NotBeNull();
      createdCustomer!.Name.Should().Be(customerCreateDto.Name);
    }

    [Fact]
    public async Task Login_ShouldReturnToken()
    {
      // Arrange - First create a customer
      var customerCreateDto = new CustomerCreateDto("Test Customer 2");
      var createResponse = await _httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
      var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerReturnDto>();

      // Act - Login doesn't require auth
      var loginResponse = await _httpClient.GetAsync($"api/customer/login/{createdCustomer!.Id}");
      var token = await loginResponse.Content.ReadAsStringAsync();

      // Assert
      loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
      token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WithAuth_ShouldReturnCustomer()
    {
      // Arrange - Create customer and login
      var customerCreateDto = new CustomerCreateDto("Test Customer 3");
      var createResponse = await _httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
      var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerReturnDto>();

      // Get auth token
      var loginResponse = await _httpClient.GetAsync($"api/customer/login/{createdCustomer!.Id}");
      var token = await loginResponse.Content.ReadAsStringAsync();
      token = token.Trim('"');

      // Act - GetById requires auth token
      var request = new HttpRequestMessage(HttpMethod.Get, $"api/customer/{createdCustomer.Id}");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

      var getResponse = await _httpClient.SendAsync(request);
      var retrievedCustomer = await getResponse.Content.ReadFromJsonAsync<CustomerReturnSingleDto>();

      // Assert
      getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
      retrievedCustomer.Should().NotBeNull();
      retrievedCustomer!.Name.Should().Be(customerCreateDto.Name);
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ShouldReturnUnauthorized()
    {
      // Act - Try to access protected endpoint without auth token
      var response = await _httpClient.GetAsync("api/customer");

      // Assert
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCustomer_WithAuth_ShouldReturnSuccess()
    {
      // Arrange - Create customer and login
      var customerCreateDto = new CustomerCreateDto("Test Customer 5");
      var createResponse = await _httpClient.PostAsJsonAsync("api/customer", customerCreateDto);
      var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerReturnDto>();

      var loginResponse = await _httpClient.GetAsync($"api/customer/login/{createdCustomer!.Id}");
      var token = await loginResponse.Content.ReadAsStringAsync();
      token = token.Trim('"');

      // Act - Delete requires auth token
      var request = new HttpRequestMessage(HttpMethod.Delete, $"api/customer/{createdCustomer.Id}");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

      var deleteResponse = await _httpClient.SendAsync(request);

      // Assert
      deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
    public Task InitializeAsync()
    {
      return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
       await Task.CompletedTask;
      // Not necessary when using an in-memory database

      //var httpClient = _factory?.CreateClient();
      //foreach (var id in _customerIds)
      //  await httpClient.DeleteAsync($"api/customer/{id}");

    }
  }
}
