using Application;
using Infrastructure;
using rental_platform.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddMediatR(configuration =>
{
  configuration.RegisterServicesFromAssemblies(typeof(Program).Assembly);

});

// // Used in Docker when HTTPS is enabled
//builder.WebHost.ConfigureKestrel(options =>
//{
//  options.ListenAnyIP(8080); // HTTP
//  options.ListenAnyIP(8081, listenOptions =>
//  {
//    listenOptions.UseHttps("/app/server.pfx", "Str0ng_Passw0rd!");
//  }); // HTTPS
//});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  await using (var serviceScope = app.Services.CreateAsyncScope())
  await using (var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
  {
    await dbContext.Database.EnsureCreatedAsync();
  }
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
  }); 
  //app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", ""));

}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();


app.Run();

public partial class Program() { }
