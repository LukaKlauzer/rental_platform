using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
  public static class DependencyInjection
  {
    public static IServiceCollection AddInfrastructureServices(
      this IServiceCollection services,
      IConfiguration config) 
    {
      // DbContext setup
      services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
          config.GetConnectionString("DefaultConnection"),
          b => b.MigrationsAssembly("Web.Api")));


      return services;
    }
  }
}
