using System.Text.Json;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using MoviesHub.Api.AppExtensions;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Middlewares;
using MoviesHub.Api.Storage;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServiceExtensions.ConfigureSerilog();

builder.Host.UseSerilog();

builder.Services.InitializeSwagger(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

builder.Services.InitializeRedis(new RedisConfig
{
    BaseUrl = builder.Configuration["RedisConfig:BaseUrl"]
});

builder.Services.AddBearerAuthentication(builder.Configuration);

builder.Services.AddCustomServicesAndConfigurations(builder.Configuration);

builder.Services.AddCors();

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddDbContext<ApplicationDbContext>(option =>
    {
        option.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"));
    },
    ServiceLifetime.Transient).AddUnitOfWork<ApplicationDbContext>();

builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

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
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();