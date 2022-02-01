using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConnectGame.Runner
{
    interface IEngineProcess : IDisposable
    {
        IList<string> History { get; }

        Task StartAsync(string path);
        Task<string> ReadLineAsync();
        Task WriteLineAsync(string line);
        void ClearHistory();
    }
}