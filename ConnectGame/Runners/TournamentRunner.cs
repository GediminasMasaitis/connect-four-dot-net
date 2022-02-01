using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConnectGame.Search;

namespace ConnectGame.Runners
{
    class TournamentRunner
    {
        private readonly Runner _runner;

        public TournamentRunner()
        {
            _runner = new Runner();
        }

        public void Run(Func<ISolver> player1Factory, Func<ISolver> player2Factory, int parallel)
        {
            var results = new List<RunnerResult>();
            var rng = new Random(0);
            //for (var i = 0; i < 1000; i++)
            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = parallel;
            const int max = 1000;
            if (parallel <= 1)
            {
                for (int iteration = 0; iteration < max; iteration++)
                {
                    RunIteration(results, rng, player1Factory, player2Factory);
                }
            }
            else
            {
                Parallel.For(0, max, options, iteration =>
                {
                    RunIteration(results, rng, player1Factory, player2Factory);
                });
            }
        }

        private void RunIteration(IList<RunnerResult> results, Random rng, Func<ISolver> player1Factory, Func<ISolver> player2Factory)
        {
            var player1 = player1Factory.Invoke();
            var player2 = player2Factory.Invoke();
            var board = new Board(7, 6);
            for (var j = 0; j < 4; j++)
            {
                var column = rng.Next(0, board.Width);
                board.MakeMove(column);
            }

            RunMatchPair(results, board, player1, player2);
        }

        public void Run(ISolver player1, ISolver player2)
        {
            var results = new List<RunnerResult>();
            var rng = new Random(0);
            for (var i = 0; i < 1000; i++)
            {
                var board = new Board(7, 6);
                for (var j = 0; j < 4; j++)
                {
                    var column = rng.Next(0, board.Width);
                    board.MakeMove(column);
                }

                RunMatchPair(results, board, player1, player2);
            }
        }

        private void RunMatchPair(IList<RunnerResult> results, Board board, ISolver player1, ISolver player2)
        {
            var result1 = RunMatch(results, board, player1, player2, false);
            var result2 = RunMatch(results, board, player1, player2, true);
            if (result1.Winner == 0 && result2.Winner == 0)
            {
            }
            else if (result1.Winner == 1 && result2.Winner == 2)
            {
            }
            else if (result1.Winner == 2 && result2.Winner == 1)
            {
            }
            else
            {
                Console.WriteLine($"MISMATCH, regular {result1.Winner}, reversed {result2.Winner}");
            }
        }

        private RunnerResult RunMatch(IList<RunnerResult> results, Board board, ISolver player1, ISolver player2, bool reverse)
        {
            board = board.Clone();
            if (reverse)
            {
                (player1, player2) = (player2, player1);
            }

            var result = _runner.Run(board, player1, player2);
            player1.ResetState();
            player2.ResetState();

            if (reverse)
            {
                var winner = result.Winner;
                if (result.Winner == 1)
                {
                    winner = 2;
                }
                else if (result.Winner == 2)
                {
                    winner = 1;
                }

                result = new RunnerResult(winner, result.Elapsed);
            }

            lock (results)
            {
                results.Add(result);
                PrintResults(results);
            }
            return result;
        }

        private void PrintResults(IList<RunnerResult> results)
        {
            var p1 = 0;
            var p2 = 0;
            var draws = 0;
            foreach (var result in results)
            {
                switch (result.Winner)
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
            var lastResult = results[^1];
            var builder = new StringBuilder();
            builder.Append($"{lastResult.Winner}   ");
            builder.Append($"[{lastResult.Elapsed.TotalSeconds:00.000}]");
            builder.Append($", P1: {p1} ({p1Percent:0.0}%)");
            builder.Append($", P2: {p2} ({p2Percent:0.0}%)");
            builder.Append($", draws {draws} ({drawPercent:0.0}%)");

            Console.WriteLine(builder);
        }

        private string GetResultString(int result)
        {
            switch (result)
            {
                case 0: return "DRAW";
                case 1: return "WIN";
                case 2: return "LOSS";
            }

            return "";
        }
    }
}