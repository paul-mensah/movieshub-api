using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Api.Services.Providers;
using Newtonsoft.Json;
using Serilog;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;

namespace MoviesHub.Api.ServiceExtensions;

public static class ServiceExtensions
{
    public static void InitializeSwagger(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));
        
        var version = configuration["SwaggerConfig:Version"];
        var title = configuration["SwaggerConfig:Title"];

        serviceCollection.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(version, new OpenApiInfo
            {
                Contact = new OpenApiContact
                {
                    Email = "paulmensah1409@gmail.com",
                    Name = "Paul Mensah",
                    Url = new Uri("https://paulmensah.dev")
                },
                Version = version,
                Title = title
            });
            c.ResolveConflictingActions(resolver => resolver.First());
            c.EnableAnnotations();
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Provide bearer token to access endpoints",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Scheme = "oauth2",
                        Name = "Bearer",
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });
    }

    public static void UseSwaggerDocumentation(this IApplicationBuilder applicationBuilder, IConfiguration configuration)
    {
        var title = configuration["SwaggerConfig:Title"];
        
        applicationBuilder.UseSwagger();
        applicationBuilder.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", title);
        });
    }
    
    public static void InitializeRedis(this IServiceCollection services, RedisConfig redisConfig)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        
        services.Configure<RedisConfig>(c =>
        {
            c.BaseUrl = redisConfig.BaseUrl;
        });
            
        var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { redisConfig.BaseUrl },
            AllowAdmin = true,
            AbortOnConnectFail = false,
            ReconnectRetryPolicy = new LinearRetry(500),
            DefaultDatabase = redisConfig.Database
        });

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
    }

    public static void AddBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    await Task.Delay(0);
                            
                    var user = context.Principal.FindFirst(c => c.Type == ClaimTypes.Thumbprint).Value;
                    var userData = JsonConvert.DeserializeObject<UserResponse>(user);
                            
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Thumbprint, JsonConvert.SerializeObject(userData)),
                    };
                            
                    var appIdentity = new ClaimsIdentity(claims, CommonConstants.AppAuthIdentity);
                    context.Principal.AddIdentity(appIdentity);
                }
            };
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = configuration["BearerTokenConfig:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["BearerTokenConfig:Key"])),
                ValidAudience = configuration["BearerTokenConfig:Audience"],
            };
        });
    }

    public static void UseSerilog(this WebApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Logging.ClearProviders();
        ILogger serilog = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        applicationBuilder.Logging.AddSerilog(serilog);
        applicationBuilder.Services.AddSingleton(serilog);
    }
    
    public static void AddCustomServicesAndConfigurations(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddScoped<IMoviesHttpService, MoviesHttpService>();
        serviceCollection.AddScoped<IMoviesService, MoviesService>();
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<IAuthService, AuthService>();

        serviceCollection.Configure<BearerTokenConfig>(configuration.GetSection(nameof(BearerTokenConfig)));
        serviceCollection.Configure<TheMovieDbConfig>(configuration.GetSection(nameof(TheMovieDbConfig)));
        serviceCollection.Configure<RedisConfig>(configuration.GetSection(nameof(RedisConfig)));
        serviceCollection.Configure<HubtelSmsConfig>(configuration.GetSection(nameof(HubtelSmsConfig)));
    }
    
}