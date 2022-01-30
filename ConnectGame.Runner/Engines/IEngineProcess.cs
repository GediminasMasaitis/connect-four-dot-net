using System;
using System.Threading.Tasks;

namespace ConnectGame.Runner
{
    interface IEngineProcess : IDisposable
    {
        Task StartAsync(string path);
        Task<string> ReadLineAsync();
        Task WriteLineAsync(string line);
    }
}