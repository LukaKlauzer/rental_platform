using Core.Domain.Entities;
using Core.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RentalPlatform.Tests.Integration.TestData
{
  public static class TestDataExtension
  {
    /// <summary>
    /// Extension method for CustomWebApplicationFactory to seed data needed for tests
    /// </summary>
    public static async Task<TestData> SeedTestDataAsync(
      this CustomWebApplicationFactory<Program> factory)
    {
      var testContext = new TestData();

      using var scope = factory.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      // Create test customer
      var firstCustomer = new Customer
      {
        Name = $"Test customer 1",
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow
      };
      var secondCustomer = new Customer
      {
        Name = $"Test customer 2",
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow
      };

      await dbContext.Customers.AddRangeAsync(firstCustomer, secondCustomer);
      await dbContext.SaveChangesAsync();
      testContext.FirstCustomerId = firstCustomer.ID;
      testContext.SecondCustomerId = secondCustomer.ID;

      // Create test vehicle
      var vehicle = new Vehicle
      {
        Vin = "VIN_123",
        Make = "Test Make",
        Model = "Test Model",
        Year = 2023,
        PricePerKmInEuro = 0.5f,
        PricePerDayInEuro = 50.0f,
        DateCreated = DateTime.UtcNow,
        DateModified = DateTime.UtcNow
      };

      await dbContext.Vehicles.AddAsync(vehicle);
      await dbContext.SaveChangesAsync();
      testContext.VehicleVin = vehicle.Vin;

      // Add telemetry data for the vehicle
      var now = DateTime.UtcNow;
      var timestamp = ((DateTimeOffset)now).ToUnixTimeSeconds();
      var odometerTelemetry = new List<Telemetry>(){
         new Telemetry
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.odometer,
          Value = 1000.0f,
          Timestamp = (int)timestamp,
          DateCreated = now,
          DateModified = now
        },
         new Telemetry
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.odometer,
          Value = 1000.0f,
          Timestamp = 1735689600,
          DateCreated = DateTime.Parse("2025-01-01T00:00:00"),
          DateModified = DateTime.Parse("2025-01-01T00:00:00")
        },
         new Telemetry
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.odometer,
          Value = 1010.0f,
          Timestamp = 1735754399,
          DateCreated = DateTime.Parse("2025-01-01T17:59:59.750000"),
          DateModified = DateTime.Parse("2025-01-01T17:59:59.750000")
        },
         new Telemetry
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.odometer,
          Value = 1020.0f,
          Timestamp = 1735819199,
          DateCreated = DateTime.Parse("2025-01-02T11:59:59.500000"),
          DateModified = DateTime.Parse("2025-01-02T11:59:59.500000")
        },
         new Telemetry
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.odometer,
          Value = 1030.0f,
          Timestamp = 1735883999,
          DateCreated = DateTime.Parse("2025-01-03T05:59:59.250000"),
          DateModified = DateTime.Parse("2025-01-03T05:59:59.250000")
        },
         new Telemetry
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.odometer,
          Value = 1040.0f,
          Timestamp = 1735948799,
          DateCreated = DateTime.Parse("2025-01-03T23:59:59"),
          DateModified = DateTime.Parse("2025-01-03T23:59:59")
        }
         };

      var batteryTelemetry = new List<Telemetry>
      {
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 87.6f,
          Timestamp = 1735689600,  // 2025-01-01 00:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 79.1f,
          Timestamp = 1735732800,  // 2025-01-01 12:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 60.5f,
          Timestamp = 1735776000,  // 2025-01-01 18:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 58.0f,
          Timestamp = 1735819200,  // 2025-01-02 00:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        },
        new Telemetry()
        {
          VehicleId = vehicle.Vin,
          Name = TelemetryType.battery_soc,
          Value = 18.2f,
          Timestamp = 1735862400,  // 2025-01-02 12:00:00 UTC
          DateCreated = new DateTime(2025, 1, 1),
          DateModified = new DateTime(2025, 1, 1)
        }
      };


      await dbContext.Telemetries.AddRangeAsync(odometerTelemetry);
      await dbContext.Telemetries.AddRangeAsync(batteryTelemetry);
      await dbContext.SaveChangesAsync();

      return testContext;
    }

    /// <summary>
    /// Extension method for CustomWebApplicationFactory to clean up test data
    /// </summary>
    public static async Task CleanupTestDataAsync(
      this CustomWebApplicationFactory<Program> factory,
      TestData testContext)
    {
      using var scope = factory.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      // Remove rentals
      var rentals = await dbContext.Rentals
          .Where(r => r.VehicleId == testContext.VehicleVin || r.CustomerId == testContext.FirstCustomerId)
          .ToListAsync();

      dbContext.Rentals.RemoveRange(rentals);

      // Remove telemetry
      var telemetry = await dbContext.Telemetries
          .Where(t => t.VehicleId == testContext.VehicleVin)
          .ToListAsync();

      dbContext.Telemetries.RemoveRange(telemetry);

      // Remove vehicle
      var vehicle = await dbContext.Vehicles.FindAsync(testContext.VehicleVin);
      if (vehicle != null)
        dbContext.Vehicles.Remove(vehicle);

      // Remove customer
      var customer = await dbContext.Customers.FindAsync(testContext.FirstCustomerId);
      if (customer != null)
        dbContext.Customers.Remove(customer);

      await dbContext.SaveChangesAsync();
    }
  }

  public class TestData
  {
    public int FirstCustomerId { get; set; }
    public int SecondCustomerId { get; set; }
    public string VehicleVin { get; set; } = string.Empty;
  }
}
