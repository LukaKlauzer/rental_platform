using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace rental_platform.Extentions
{
  public static class ServiceCollectionExtensions
  {
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
      services.AddSwaggerGen(options =>
      {
        var securityScheme = new OpenApiSecurityScheme
        {
          Name = "JWT Authentification",
          Description = "Enter JWT token",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.Http,
          Scheme = "bearer",//JwtBearerDefaults.AuthenticationScheme,
          BearerFormat = JwtBearerDefaults.AuthenticationScheme,

        };
        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

        var securityRequierment = new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme{
              Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
              }
            },[]
          }
        };
        options.AddSecurityRequirement(securityRequierment);
      });

      return services;
    }

  }
}
