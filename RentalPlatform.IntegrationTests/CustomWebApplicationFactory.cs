using System.ComponentModel;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RentalPlatform.Tests.Integration
{
  public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
  {
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureServices(services =>
      {
        // Remove any existing DbContext registrations
        var descriptorsToRemove = services
            .Where(d =>
                d.ServiceType.Name.Contains("DbContext") ||
                (d.ServiceType.Name.Contains("Options") && d.ServiceType.Name.Contains("DbContext")) ||
                (d.ImplementationType?.Name.Contains("DbContext") ?? false))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
          services.Remove(descriptor);
        }

        // Add in-memory database context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
          options.UseInMemoryDatabase(_databaseName);
          options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Initialize the database
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
          var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
          db.Database.EnsureCreated();
        }
      });
    }
  }
}
