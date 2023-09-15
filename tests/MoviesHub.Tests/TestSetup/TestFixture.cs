using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoviesHub.Api.AppExtensions;

namespace MoviesHub.Tests.TestSetup;

public class TestFixture
{
    public ServiceProvider ServiceProvider { get; }
    
    public TestFixture()
    {
        var services = new ServiceCollection();
        ConfigurationManager.SetupConfiguration();

        services
            .AddSingleton(sp => ConfigurationManager.Configuration)
            .AddLogging(x => x.AddConsole())
            .AddCustomServicesAndConfigurations(ConfigurationManager.Configuration);
        
        ServiceProvider = services.BuildServiceProvider();
    }
}