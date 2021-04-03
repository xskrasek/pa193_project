using Microsoft.Extensions.DependencyInjection;

namespace PA193_Project
{
    class CertParser
    {
        static void Main(string[] args)
        {
            // Set up Dependency Injection
            var services = Startup.ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<EntryPoint>().Run(args);
        }

    }
}
