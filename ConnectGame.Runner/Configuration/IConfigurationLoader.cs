using System.Threading.Tasks;

namespace ConnectGame.Runner.Configuration
{
    internal interface IConfigurationLoader
    {
        Task<RootConfig> LoadAsync();
    }
}