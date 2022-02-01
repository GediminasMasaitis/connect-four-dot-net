using System;
using System.Data;
using System.Threading;
using ConnectGame.Eval;
using ConnectGame.Search;

namespace ConnectGame
{
    class Uc4iRunner
    {
        private readonly ISolver _solver;
        private readonly BoardParser _parser;
        private readonly BoardVisualizer _visualizer;

        private const string Version = "1.0.2";

        public Uc4iRunner()
        {
            //var evaluation = new Evaluation();
            var evaluation = new EvaluationNew();
            _solver = new Solver(evaluation);
            //_solver = new RandomSolver();
            _parser = new BoardParser();
            _visualizer = new BoardVisualizer(evaluation);
        }

        public void Run()
        {
            Board board = new Board(Rules.Width, Rules.Height);
            while (true)
            {
                var line = Console.ReadLine();
                var ok = HandleLine(ref board, line);
                if (!ok)
                {
                    break;
                }
            }
        }

        private bool HandleLine(ref Board board, string line)
        {
            if (line == null)
            {
                return false;
            }

            if (line == "uc4i")
            {
                Console.WriteLine("id name ConnectGame");
                Console.WriteLine($"id version {Version}");
                Console.WriteLine("id author Gediminas Masaitis");
                Console.WriteLine("uc4iok");
                return true;
            }

            if (line == "uc4inewgame")
            {
                board = null;
                _solver.ResetState();
                return true;
            }

            if (line.StartsWith("position"))
            {
                board = _parser.Parse(line);
                return true;
            }

            if (line.StartsWith("m ") || line.StartsWith("move "))
            {
                var words = line.Split(' ');
                if (words.Length != 2)
                {
                    Console.WriteLine("Incorrect makemove format");
                    return true;
                }


                var moveStr = words[1];
                var isValidMove = int.TryParse(moveStr, out var move) && move >= 0 && move < board.Width;
                if (!isValidMove)
                {
                    Console.WriteLine($"Invalid move {moveStr}");
                    return true;
                }

                board.MakeColumn(move);
                return true;
            }

            if (line == "d")
            {
                board = new Board(Rules.Width, Rules.Height);
                board.MakeColumn(0);
                board.MakeColumn(2);
                board.MakeColumn(0);
                board.MakeColumn(4);
                board.MakeColumn(0);
                board.MakeColumn(6);
                board.MakeColumn(0);
                return true;
            }

            if (line == "u" || line == "undo")
            {
                board.UnmakeMove();
                return true;
            }

            if (line == "isready")
            {
                Console.WriteLine("readyok");
                return true;
            }

            if (line.StartsWith("go"))
            {
                HandleGo(board, line);
                return true;
            }

            if (line == "b")
            {
                var boardStr = _visualizer.Visualize(board);
                Console.WriteLine(boardStr);
                return true;
            }

            if (line == "gi")
            {
                return HandleLine(ref board, "go infinite");
            }

            if (line == "exit")
            {
                return false;
            }

            Console.WriteLine("Unknown command");
            return true;
        }

        private void HandleGo(Board board, string line)
        {
            var words = line.Split(' ');
            var searchParams = new SearchParameters();
            for (var i = 0; i < words.Length; i++)
            {
                switch (words[i])
                {
                    case "wtime":
                        searchParams.WhiteTime = long.Parse(words[++i]);
                        break;
                    case "btime":
                        searchParams.BlackTime = long.Parse(words[++i]);
                        break;
                    case "winc":
                        searchParams.WhiteTimeIncrement = long.Parse(words[++i]);
                        break;
                    case "binc":
                        searchParams.BlackTimeIncrement = long.Parse(words[++i]);
                        break;
                    case "infinite":
                        searchParams.Infinite = true;
                        break;
                    case "depth":
                        searchParams.MaxDepth = int.Parse(words[++i]);
                        break;
                }
            }

            var column = _solver.Solve(board, searchParams, CancellationToken.None);
            Console.WriteLine($"bestmove {column}");
        }
    }
}