using Arch.EntityFrameworkCore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoviesHub.Api.AppExtensions;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Storage;

namespace MoviesHub.Tests.TestSetup;

public class TestFixture
{
    public ServiceProvider ServiceProvider { get; }
    
    public TestFixture()
    {
        var services = new ServiceCollection();
        ConfigurationManager.SetupConfiguration();

        services.AddSingleton(sp => ConfigurationManager.Configuration);

        services.AddLogging(x => x.AddConsole());
        //
        services.InitializeRedis(new RedisConfig
        {
            BaseUrl = ConfigurationManager.Configuration["RedisConfig:BaseUrl"]
        });
        
        services.AddCustomServicesAndConfigurations(ConfigurationManager.Configuration);

        services.AddBearerAuthentication(ConfigurationManager.Configuration);
        
        services.AddDbContext<ApplicationDbContext>(x =>
        {
            x.UseInMemoryDatabase("holidayLaundryDb");
        }, ServiceLifetime.Transient).AddUnitOfWork<ApplicationDbContext>();
        
        ServiceProvider = services.BuildServiceProvider();
    }
}