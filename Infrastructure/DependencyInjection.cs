using System.Text;
using Core.Domain.Entities;
using Core.Interfaces.Authentification;
using Core.Interfaces.Persistence.GenericRepository;
using Core.Interfaces.Persistence.SpecificRepository;
using Infrastructure.Authentification;
using Infrastructure.Persistance.ConcreteRepositories;
using Infrastructure.Persistance.GenericRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
          b => b.MigrationsAssembly("Infrastructure")
          ));

      services.AddAuth(config);

      services.AddScoped<IRepository<Telemetry>, Repository<Telemetry>>();
      services.AddScoped<IRepository<Customer>, Repository<Customer>>();
      services.AddScoped<IRepository<Vehicle>, Repository<Vehicle>>();
      services.AddScoped<IRepository<Rental>, Repository<Rental>>();

      services.AddScoped<IVehicleRepository, VehicleRepository>();
      services.AddScoped<ITelemetryRepository, TelemetryRepository>();
      services.AddScoped<ICustomerRepository, CustomerRepository>();
      services.AddScoped<IRentalRepository, RentalRepository>();


      return services;
    }

    public static IServiceCollection AddAuth(
      this IServiceCollection services,
      IConfiguration config)
    {
      var jwtSettings = new JwtSettings();

      config.Bind(JwtSettings.SectionName, jwtSettings);

      services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
      services.AddSingleton(Options.Create(jwtSettings));
      services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.RequireHttpsMetadata = false;
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            ClockSkew = TimeSpan.Zero,
          };

        });

      return services;
    }
  }
}
