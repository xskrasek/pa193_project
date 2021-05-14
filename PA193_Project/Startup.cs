using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PA193_Project.Services;

namespace PA193_Project
{
    public static class Startup
    {
        public static IServiceCollection ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IParserService, ParserService>();
            serviceCollection.AddLogging(builder =>
            {
                builder
                .AddFilter("PA193_Project", LogLevel.Warning)
                .AddConsole();
            });
            serviceCollection.AddTransient<EntryPoint>();

            return serviceCollection;
        }
    }
}
