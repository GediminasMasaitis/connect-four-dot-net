using System.Threading.Tasks;
using ConnectGame.Runner.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectGame.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var startup = new Startup();
            var services = new ServiceCollection();
            startup.ConfigureServices(services);
            var provider = services.BuildServiceProvider();

            var loader = provider.GetRequiredService<IConfigurationLoader>();
            ConfigurationHolder.Config = await loader.LoadAsync();
            var tournamentRunner = provider.GetRequiredService<ITournamentRunner>();
            await tournamentRunner.RunAsync();
        }
    }
}
