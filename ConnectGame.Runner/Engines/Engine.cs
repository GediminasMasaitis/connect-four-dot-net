using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectGame.Runner.Game;
using Microsoft.Extensions.Logging;

namespace ConnectGame.Runner.Engines
{
    class Engine : IEngine
    {
        public IList<string> History => _process.History;

        public EngineInfo Info { get; }
        public Task ReadLoop { get; private set; }

        private readonly ILogger<Engine> _logger;
        private readonly IEngineProcess _process;
        private readonly IEngineHandlerManager _handlerManager;
        private readonly IBoardSerializer _serializer;
        private readonly IDictionary<string, object> _loggingScopeState;

        public Engine
        (
            ILogger<Engine> logger,
            IEngineProcess process,
            IEngineHandlerManager handlerManager,
            IBoardSerializer serializer
        )
        {
            _logger = logger;
            _process = process;
            _handlerManager = handlerManager;
            _serializer = serializer;

            _loggingScopeState = new Dictionary<string, object>();
            Info = new EngineInfo();
        }

        public void Dispose()
        {
            _process.Dispose();
        }

        public async Task RunAsync(int id, string path, CancellationToken token)
        {
            using (_logger.BeginScope(_loggingScopeState))
            {
                Info.Id = id;
                Info.Path = path;

                _loggingScopeState["EngineId"] = id;

                _handlerManager.AddHandlerAsync(HandleIdAsync);

                await _process.StartAsync(path);
                ReadLoop = ReadLoopAsync(token);
            }
        }

        private async Task ReadLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var message = await _process.ReadLineAsync();
                await HandleMessageAsync(message);
            }
        }

        public async Task SendUc4iAsync()
        {
            await SendAndWait("uc4i", async message => message == "uc4iok");
        }

        public async Task SendUc4iNewGame()
        {
            await _process.WriteLineAsync("uc4inewgame");
        }

        public async Task SendPosition(Board board)
        {
            var boardString = _serializer.Serialize(board);
            await _process.WriteLineAsync(boardString);
        }

        public async Task SendIsReady()
        {
            await SendAndWait("isready", async message => message == "readyok");
        }

        public async Task<int?> SendGo(IMatchTimeControl timeControl)
        {
            //_logger.LogInformation(Info.Version);
            string bestmoveMessage = null;
            //wtime 10000 winc 100 btime 10000 binc 100
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var goBuilder = new StringBuilder();
            goBuilder.Append("go");
            var hasTime = false;
            if (timeControl.Times[1].HasValue)
            {
                hasTime = true;
                goBuilder.Append($" wtime {(int)timeControl.Times[1].Value.TotalMilliseconds}");
            }
            if (timeControl.Times[2].HasValue)
            {
                hasTime = true;
                goBuilder.Append($" btime {(int)timeControl.Times[2].Value.TotalMilliseconds}");
            }
            if (timeControl.Config.Increment.HasValue)
            {
                goBuilder.Append($" winc {timeControl.Config.Increment.Value}");
                goBuilder.Append($" binc {timeControl.Config.Increment.Value}");
            }
            if (!hasTime)
            {
                goBuilder.Append(" infinite");
            }

            if (timeControl.Config.Depth.HasValue)
            {
                goBuilder.Append($" depth {timeControl.Config.Depth.Value}");
            }

            var goStr = goBuilder.ToString();
            await SendAndWait(goStr, async message =>
            {
                var isBestMove = message.StartsWith("bestmove");
                if (!isBestMove)
                {
                    return false;
                }

                bestmoveMessage = message;
                return true;
            });
            //stopwatch.Stop();
            //_logger.LogInformation(stopwatch.Elapsed.TotalMilliseconds.ToString());

            var words = bestmoveMessage.Split(' ');
            if (words.Length != 2)
            {
                _logger.LogError("Incorrect bestmove command format");
                return null;
            }

            var isIntegerColumn = int.TryParse(words[1], out var column);
            if(!isIntegerColumn)
            {
                _logger.LogError("Incorrect bestmove column format");
                return null;
            }

            //if (column < 0)
            //{
            //    _logger.LogError("Incorrect column");
            //    return null;
            //}

            return column;
        }

        private async Task SendAndWait(string message, Func<string, Task<bool>> condition)
        {
            var waitTask = _handlerManager.AddConditionAsync(condition);
            await _process.WriteLineAsync(message);
            await waitTask;
        }

        private async Task HandleMessageAsync(string message)
        {
            await _handlerManager.RunHandlersAsync(message);
        }

        private async Task HandleIdAsync(EngineHandlerParameters parameters)
        {
            if (!parameters.Message.StartsWith("id "))
            {
                return;
            }

            parameters.IsHandled = true;

            var words = parameters.Message.Split(' ');
            if (words.Length < 3)
            {
                _logger.LogWarning("Incorrect id command format");
                return;
            }

            switch (words[1])
            {
                case "name":
                    Info.Name = words[2];
                    _loggingScopeState["EngineName"] = Info.Name;
                    break;
                case "version":
                    Info.Version = words[2];
                    _loggingScopeState["EngineVersion"] = Info.Version;
                    break;
                case "author":
                    Info.Author = string.Join(" ", words.Skip(2));
                    break;
                default:
                    _logger.LogWarning("Unknown id subcommand {Subcommand}", words[1]);
                    break;
            }
        }

        public void ClearHistory()
        {
            _process.ClearHistory();
        }
    }
}