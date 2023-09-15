using System.Reflection;
using System.Security.Claims;
using System.Text;
using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Helpers;
using MoviesHub.Api.Models.Response;
using MoviesHub.Api.Repositories;
using MoviesHub.Api.Services.Interfaces;
using MoviesHub.Api.Services.Providers;
using MoviesHub.Api.Storage;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using StackExchange.Redis;

namespace MoviesHub.Api.AppExtensions;

public static class ServiceExtensions
{
    private static IServiceCollection InitializeSwagger(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));
        
        string version = configuration["SwaggerConfig:Version"];
        string title = configuration["SwaggerConfig:Title"];

        serviceCollection.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(version, new OpenApiInfo
            {
                Contact = new OpenApiContact
                {
                    Email = "paulmensah1409@gmail.com",
                    Name = "Paul Mensah"
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
            
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        return serviceCollection;
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

    private static IServiceCollection InitializeRedis(this IServiceCollection services, Action<RedisConfig> redisConfig)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));

        services.Configure(redisConfig);

        var redisConfiguration = new RedisConfig();
        redisConfig.Invoke(redisConfiguration);
        
        var connectionMultiplexer = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { redisConfiguration.BaseUrl },
            AllowAdmin = true,
            AbortOnConnectFail = false,
            ReconnectRetryPolicy = new LinearRetry(500),
            DefaultDatabase = redisConfiguration.Database
        });

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
        services.AddSingleton<IRedisService, RedisService>();

        return services;
    }

    private static IServiceCollection AddBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
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
                            
                    string user = context.Principal.FindFirst(c => c.Type == ClaimTypes.Thumbprint).Value;
                    UserResponse userData = JsonConvert.DeserializeObject<UserResponse>(user);
                            
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Thumbprint, JsonConvert.SerializeObject(userData)),
                    };
                            
                    ClaimsIdentity appIdentity = new ClaimsIdentity(claims, CommonConstants.Authentication.AppAuthIdentity);
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

        return services;
    }

    public static void ConfigureSerilog()
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                optional: true)
            .Build();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticSearchConfig:Url"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name!.ToLower().Replace(".", "-")}-" +
                              $"{environment.ToLower().Replace(".", "-")}"
            })
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
    
    public static IServiceCollection AddCustomServicesAndConfigurations(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .Configure<BearerTokenConfig>(configuration.GetSection(nameof(BearerTokenConfig)))
            .Configure<TheMovieDbConfig>(configuration.GetSection(nameof(TheMovieDbConfig)))
            .Configure<RedisConfig>(configuration.GetSection(nameof(RedisConfig)))
            .Configure<HubtelSmsConfig>(configuration.GetSection(nameof(HubtelSmsConfig)));
        
        serviceCollection.InitializeSwagger(configuration)
            .InitializeRedis(c => configuration.GetSection(nameof(RedisConfig)).Bind(c))
            .AddBearerAuthentication(configuration)
            .AddDbContext<ApplicationDbContext>(option =>
            {
                option.UseNpgsql(configuration.GetConnectionString("DbConnection"));
            },
            ServiceLifetime.Transient).AddUnitOfWork<ApplicationDbContext>();

        serviceCollection
            .AddScoped<IMoviesHttpService, MoviesHttpService>()
            .AddScoped<IMoviesService, MoviesService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<ISmsService, SmsService>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserCacheRepository, UserCacheRepository>()
            .AddScoped<IFavoriteMovieRepository, FavoriteMovieRepository>()
            .AddScoped<IOtpCodeRepository, OtpCodeRepository>();
        
        return serviceCollection;
    }
    
    public static async Task RunMigrations(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
            int pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).Count();
            
            if (pendingMigrations >= 1)
            {
                await dbContext.Database.MigrateAsync();
                logger.LogDebug("{pendingMigrations} migrations successfully executed ", pendingMigrations);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured executing pending migrations");
        }
    }
    
}