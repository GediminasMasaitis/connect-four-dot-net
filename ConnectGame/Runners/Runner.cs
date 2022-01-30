using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectGame.Search;

namespace ConnectGame
{
    class Runner
    {
        private readonly WinDetector _win;

        public Runner(bool printTimes = false)
        {
            _win = new WinDetector();
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
                var winner = _win.GetWinner(board);
                var entry = new RunnerEntry(winner, iterationStopwatch.Elapsed);
                onIteration?.Invoke(entry);
                if (winner.HasValue)
                {
                    matchStopwarch.Stop();
                    var result = new RunnerResult(winner.Value, matchStopwarch.Elapsed);
                    return result;
                }
            }

            throw new Exception("Unable to get winner");
        }

        //private int? GetWinner(Board board)
        //{
        //    for (int column = 0; column < board.Width; column++)
        //    {
        //        if (board.Fills[column] == board.Cells[column].Length)
        //        {
        //            continue;
        //        }
        //    }
        //}

        private bool IsFilled(Board board)
        {
            for (int column = 0; column < board.Width; column++)
            {
                if (board.Fills[column] != board.Cells[column].Length)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
