using System.Text.Json;
using MoviesHub.Api.AppExtensions;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServiceExtensions.ConfigureSerilog();
builder.Host.UseSerilog();
builder.Services.InitializeSwagger(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.InitializeRedis(new RedisConfig
{
    BaseUrl = builder.Configuration["RedisConfig:BaseUrl"]
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddBearerAuthentication(builder.Configuration);
builder.Services.AddCustomServicesAndConfigurations(builder.Configuration);
builder.Services.AddCors();
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

var app = builder.Build();

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