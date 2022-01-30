using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace ConnectGame.Runner.Configuration
{
    class ConfigurationLoader : IConfigurationLoader
    {
        private readonly IDeserializer _deserializer;

        public ConfigurationLoader()
        {
            var builder = new DeserializerBuilder();
            _deserializer = builder.Build();
        }

        public async Task<RootConfig> LoadAsync()
        {
            var yaml = await File.ReadAllTextAsync("config.yaml");
            var config = _deserializer.Deserialize<RootConfig>(yaml);
            return config;
        }
    }
}