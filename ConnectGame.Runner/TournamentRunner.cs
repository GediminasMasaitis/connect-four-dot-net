using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectGame.Runner.Configuration;
using ConnectGame.Runner.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConnectGame.Runner
{
    record TournamentEntry(int? Winner, TimeSpan Elapsed, TimeSpan?[] RemainingTimes);

    public class SearchParameters
    {
        public bool Infinite { get; set; }
        public int? MaxDepth { get; set; }

        public long WhiteTime { get; set; }
        public long BlackTime { get; set; }

        public long WhiteTimeIncrement { get; set; }
        public long BlackTimeIncrement { get; set; }

        public SearchParameters()
        {
            Infinite = false;
            MaxDepth = null;
            WhiteTime = 0;
            BlackTime = 0;
            WhiteTimeIncrement = 0;
            BlackTimeIncrement = 0;
        }
    }

    class TournamentRunner : ITournamentRunner
    {
        private readonly ILogger<TournamentRunner> _logger;
        private readonly RootConfig _config;
        private readonly IServiceProvider _provider;
        private readonly IWinDetector _winDetector;
        private readonly Random _rng;

        public TournamentRunner
        (
            ILogger<TournamentRunner> logger,
            RootConfig config,
            IServiceProvider provider,
            IWinDetector winDetector
        )
        {
            _logger = logger;
            _config = config;
            _provider = provider;
            _winDetector = winDetector;
            _rng = new Random(0);
        }

        public async Task RunAsync()
        {
            const int matchCount = 2000;

            var cts = new CancellationTokenSource();

            var tasks = new HashSet<Task<IList<IEngine>>>();
            for (int threadId = 0; threadId < _config.ThreadCount; threadId++)
            {
                var task = PrepareEngines(cts.Token);
                tasks.Add(task);
            }

            var results = new List<TournamentEntry>();
            for (int matchId = 0; matchId < matchCount; matchId++)
            {
                var completedTask = await Task.WhenAny(tasks);
                var engines = await completedTask;
                tasks.Remove(completedTask);

                var newTask = Task.Run(async () => await RunTaskAsync(matchId, engines, results));
                tasks.Add(newTask);
            }

            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                await completedTask;
                tasks.Remove(completedTask);
            }
        }

        private async Task<IList<IEngine>> PrepareEngines(CancellationToken token)
        {
            var engines = new List<IEngine>(2);

            for (var engineId = 0; engineId < _config.Engines.Count; engineId++)
            {
                var engineConfig = _config.Engines[engineId];
                var engine = _provider.GetRequiredService<IEngine>();
                engines.Add(engine);
                await engine.RunAsync(engineId, engineConfig.Path, token);
                await engine.SendUc4iAsync();
            }

            return engines;
        }

        public async Task<IList<IEngine>> RunTaskAsync(int matchId, IList<IEngine> engines, IList<TournamentEntry> results)
        {
            var board = GetStartingBoard();
            //var reversed = GetReversedBoard(board);
            var clone = board.Clone();

            var result = await RunMatchAsync(engines, board);
            var reversedResult = await RunReverseMatchAsync(engines, clone);

            lock (results)
            {
                results.Add(result);
                results.Add(reversedResult);
            }

            lock (results)
            {
                PrintResults(matchId, results, result, reversedResult);
            }

            return engines;
        }

        private void PrintResults(int matchId, IList<TournamentEntry> results, TournamentEntry result, TournamentEntry reversedResult)
        {
            var p1 = 0;
            var p2 = 0;
            var draws = 0;
            foreach (var historyResult in results)
            {
                switch (historyResult.Winner)
                {
                    case 0:
                        draws++;
                        break;
                    case 1:
                        p1++;
                        break;
                    case 2:
                        p2++;
                        break;
                }
            }

            var p1Percent = 100 * p1 / (double)results.Count;
            var p2Percent = 100 * p2 / (double)results.Count;
            var drawPercent = 100 * draws / (double)results.Count;
            //var elo = CalculateElo(results);
            var elo = new Elo(p1, p2, draws);
            var eloValue = elo.diff();
            var eloStr = (eloValue > 0 ? "+" : string.Empty) + $"{eloValue:0.00}";
            var eloMargin = elo.errorMargin();
            var los = elo.LOS();

            var remainingE1reg = result.RemainingTimes[0]?.ToString("ss\\.fff") ?? "-";
            var remainingE2reg = result.RemainingTimes[1]?.ToString("ss\\.fff") ?? "-";
            var remainingE1rev = reversedResult.RemainingTimes[0]?.ToString("ss\\.fff") ?? "-";
            var remainingE2rev = reversedResult.RemainingTimes[1]?.ToString("ss\\.fff") ?? "-";

            _logger.LogInformation(
                "[{MatchId:0000}] {Elo} +- {EloMargin:0.00}, LOS: {Los}, E1: {Engine1Wins} ({Engine1Percent:00.0}%), E2: {Engine2Wins} ({Engine2Percent:00.0}%), Draw: {Draws} ({DrawPercent:00.0}%), Results: {ResultRegular} {ResultReversed}, elapsed: {ElapsedRegular:g} {ElapsedReversed:g}, remaining: {RemainingTimeE1Regular} {RemainingTimeE2Regular} {RemainingTimeE1Reversed} {RemainingTimeE2Reversed}",
                matchId,
                eloStr,
                eloMargin,
                los,
                p1,
                p1Percent,
                p2,
                p2Percent,
                draws,
                drawPercent,
                result.Winner,
                reversedResult.Winner,
                result.Elapsed,
                reversedResult.Elapsed,
                remainingE1reg,
                remainingE2reg,
                remainingE1rev,
                remainingE2rev
            );
        }

        private Board GetStartingBoard()
        {
            var board = new Board(7, 6);
            lock (_rng)
            {
                for (int i = 0; i < 4; i++)
                {
                    var move = _rng.Next(0, board.Width);
                    board.MakeMove(move);
                }
            }
            return board;
        }

        private Board GetReversedBoard(Board board)
        {
            var reversed = new Board(board.Width, board.Height);
            reversed.SwapPlayers();
            foreach (var move in board.History)
            {
                reversed.MakeMove(move);
            }

            return reversed;
        }

        private async Task<TournamentEntry> RunReverseMatchAsync(IList<IEngine> engines, Board board)
        {
            var reversedEngines = new List<IEngine> { engines[1], engines[0] };
            var result = await RunMatchAsync(reversedEngines, board);

            (result.RemainingTimes[0], result.RemainingTimes[1]) = (result.RemainingTimes[1], result.RemainingTimes[0]);


            switch (result.Winner)
            {
                case 1:
                    result = new TournamentEntry(2, result.Elapsed, result.RemainingTimes);
                    break;
                case 2:
                    result = new TournamentEntry(1, result.Elapsed, result.RemainingTimes);
                    break;
            }

            return result;
        }

        private async Task<TournamentEntry> RunMatchAsync(IList<IEngine> engines, Board board)
        {
            var timeControl = _provider.GetRequiredService<IMatchTimeControl>();
            timeControl.Reset();
            foreach (var engine in engines)
            {
                await engine.SendUc4iNewGame();
                await engine.SendIsReady();
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var moveNr = 0; ; moveNr++)
            {
                var timeWinner = timeControl.GetWinner();
                if (timeWinner.HasValue)
                {
                    stopwatch.Stop();
                    var times = new TimeSpan?[2];
                    times[0] = timeControl.Times[1];
                    times[1] = timeControl.Times[2];
                    var result = new TournamentEntry(timeWinner.Value, stopwatch.Elapsed, times);
                    return result;
                }

                var winner = _winDetector.GetWinner(board);
                if (winner.HasValue)
                {
                    stopwatch.Stop();
                    var times = new TimeSpan?[2];
                    times[0] = timeControl.Times[1];
                    times[1] = timeControl.Times[2];
                    var result = new TournamentEntry(winner.Value, stopwatch.Elapsed, times);
                    return result;
                }

                var engineId = moveNr % 2;
                var engine = engines[engineId];
                await engine.SendPosition(board);
                await engine.SendIsReady();
                int? column = null;
                await timeControl.TimeAsync(board.Player, async () =>
                {
                    column = await engine.SendGo(timeControl);
                });
                
                if (!column.HasValue)
                {
                    _logger.LogError("Engine failed to provide column");
                    Environment.Exit(1);
                }

                board.MakeMove(column.Value);
            }
        }
    }

    internal interface IMatchTimeControl
    {
        TimeControlConfig Config { get; }
        TimeSpan?[] Times { get; }
        void Reset();
        Task<bool> TimeAsync(int player, Func<Task> action);
        int? GetWinner();
    }

    class MatchTimeControl : IMatchTimeControl
    {
        public TimeControlConfig Config { get; }
        public TimeSpan?[] Times { get; }

        public MatchTimeControl(TimeControlConfig config)
        {
            Config = config;
            Times = new TimeSpan?[3];
        }

        public void Reset()
        {
            for (int i = 0; i < 3; i++)
            {
                Times[i] = Config.Time.HasValue ? TimeSpan.FromMilliseconds(Config.Time.Value) : null;
            }
        }

        public async Task<bool> TimeAsync(int player, Func<Task> action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await action.Invoke();
            stopwatch.Stop();
            Times[player] -= stopwatch.Elapsed;
            var inTime = Times[player] > TimeSpan.Zero;
            if (!inTime)
            {
                return false;
            }

            if (Times[player] != null && Config.Increment.HasValue)
            {
                Times[player] += TimeSpan.FromMilliseconds(Config.Increment.Value);
            }

            return true;
        }

        public int? GetWinner()
        {
            if (Times[1].HasValue && Times[1].Value <= TimeSpan.Zero)
            {
                return 2;
            }

            if (Times[2].HasValue && Times[2].Value <= TimeSpan.Zero)
            {
                return 1;
            }

            return null;
        }
    }
}