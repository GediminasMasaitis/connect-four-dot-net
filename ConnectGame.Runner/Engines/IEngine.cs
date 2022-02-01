using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConnectGame.Runner.Game;

namespace ConnectGame.Runner
{
    internal interface IEngine : IDisposable
    {
        IList<string> History { get; }

        EngineInfo Info { get; }
        Task ReadLoop { get; }

        Task RunAsync(int id, string path, CancellationToken token);

        Task SendUc4iAsync();
        Task SendUc4iNewGame();
        Task SendPosition(Board board);
        Task SendIsReady();
        Task<int?> SendGo(IMatchTimeControl timeControl);
        void ClearHistory();
    }
}