using System.Text.Json;
using MoviesHub.Api.AppExtensions;
using MoviesHub.Api.Middlewares;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServiceExtensions.ConfigureSerilog();
builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);
builder.Services.AddCustomServicesAndConfigurations(builder.Configuration);

WebApplication app = builder.Build();

// Check if there are pending migrations and execute
app.Services.RunMigrations().GetAwaiter().GetResult();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(app.Configuration);
}

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseRouting();
app.ConfigureGlobalHandler(app.Logger);
app.UseAuthorization();
app.MapControllers();

app.Run();