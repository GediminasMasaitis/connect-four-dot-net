using System;
using System.Diagnostics;
using System.Threading;
using ConnectGame.Eval;
using ConnectGame.Search;

namespace ConnectGame.Runners
{
    class Runner
    {
        private readonly IEvaluation _eval;

        public Runner()
        {
            _eval = new Evaluation();
        }

        public RunnerResult Run(Board board, ISolver player1, ISolver player2, Action<RunnerEntry> onIteration = default)
        {
            var iterationStopwatch = new Stopwatch();
            var matchStopwarch = new Stopwatch();
            matchStopwarch.Start();
            var players = new ISolver[]
            {
                null,
                player1,
                player2
            };

            while (true)
            {
                if (IsFilled(board))
                {
                    break;
                }

                iterationStopwatch.Restart();
                var parameters = new SearchParameters();
                parameters.Infinite = true;
                parameters.MaxDepth = 4;
                var column = players[board.Player].Solve(board, parameters, CancellationToken.None);
                iterationStopwatch.Stop();
                board.MakeMove(column);
                var eval = _eval.Evaluate(board, out var winner);
                var entry = new RunnerEntry(winner, iterationStopwatch.Elapsed);
                onIteration?.Invoke(entry);
                if (winner != -1)
                {
                    matchStopwarch.Stop();
                    var result = new RunnerResult(winner, matchStopwarch.Elapsed);
                    return result;
                }
            }

            throw new Exception("Unable to get winner");
        }

        private bool IsFilled(Board board)
        {
            foreach (var fill in board.Fills)
            {
                if (fill != board.Height)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
