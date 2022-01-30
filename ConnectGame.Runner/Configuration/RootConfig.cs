using System.Collections.Generic;

namespace ConnectGame.Runner.Configuration
{
    class RootConfig
    {
        public int ThreadCount { get; set; }
        public IList<EngineConfig> Engines { get; set; }
        public TimeControlConfig TimeControl { get; set; }
    }
}
