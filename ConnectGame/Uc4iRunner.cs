using System;
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
            var evaluation = new Evaluation();
            //var evaluation = new EvaluationNew();
            _solver = new Solver(evaluation);
            //_solver = new RandomSolver();
            _parser = new BoardParser();
            _visualizer = new BoardVisualizer(evaluation);
        }

        public void Run()
        {
            Board board = new Board(7, 6);
            while (true)
            {
                var line = Console.ReadLine();
                if (line == null)
                {
                    return;
                }

                if (line == "uc4i")
                {
                    Console.WriteLine("id name ConnectGame");
                    Console.WriteLine($"id version {Version}");
                    Console.WriteLine("id author Gediminas Masaitis");
                    Console.WriteLine("uc4iok");
                    continue;
                }

                if (line == "uc4inewgame")
                {
                    board = null;
                    _solver.ResetState();
                    continue;
                }

                if (line.StartsWith("position"))
                {
                    board = _parser.Parse(line);
                    continue;
                }

                if (line.StartsWith("m "))
                {
                    var words = line.Split(' ');
                    if (words.Length != 2)
                    {
                        Console.WriteLine("Incorrect makemove format");
                        continue;
                    }


                    var moveStr = words[1];
                    var isValidMove = int.TryParse(moveStr, out var move) && move >= 0 && move < board.Width;
                    if (!isValidMove)
                    {
                        Console.WriteLine($"Invalid move {moveStr}");
                        continue;
                    }

                    board.MakeMove(move);
                    continue;
                }

                if (line == "d")
                {
                    board = new Board(7, 6);
                    board.MakeMove(0);
                    board.MakeMove(2);
                    board.MakeMove(0);
                    board.MakeMove(4);
                    board.MakeMove(0);
                    board.MakeMove(6);
                    board.MakeMove(0);
                    continue;
                }

                if (line == "u")
                {
                    board.UnmakeMove();
                    continue;
                }

                if (line == "isready")
                {
                    Console.WriteLine("readyok");
                    continue;
                }

                if (line.StartsWith("go"))
                {
                    HandleGo(board, line);
                    continue;
                }

                if (line == "b")
                {
                    var boardStr = _visualizer.Visualize(board);
                    Console.WriteLine(boardStr);
                    continue;
                }

                if (line == "exit")
                {
                    return;
                }

                Console.WriteLine("Unknown command");
            }
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