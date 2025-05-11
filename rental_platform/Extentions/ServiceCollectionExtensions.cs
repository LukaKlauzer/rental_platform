using Application.Interfaces.Authentification;
using rental_platform.Authentification;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace rental_platform.Extensions
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

    internal static IServiceCollection AddAuth(
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
