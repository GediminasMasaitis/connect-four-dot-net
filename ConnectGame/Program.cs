using System;
using ConnectGame.Eval;
using ConnectGame.Runners;
using ConnectGame.Search;

namespace ConnectGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Zobrist.Init(Rules.Width, Rules.Height);
            RunUc4i();
            //RunTest();
            //RunTournament();
        }

        static void RunUc4i()
        {
            var runner = new Uc4iRunner();
            runner.Run();
        }

        static void RunTest()
        {
            var board = new Board(Rules.Width, Rules.Height);
            var evaluation = new Evaluation();
            var visualizer = new BoardVisualizer(evaluation);

            var runner = new Runner();
            var eval1 = new Evaluation();
            var solver1 = new Solver(eval1, 10);

            var eval2 = new Evaluation();
            var solver2 = new Solver(eval2, 10);

            var result = runner.Run(board, solver1, solver2, entry =>
            {
                Console.WriteLine("Elapsed: " + entry.Elapsed.TotalSeconds);
                visualizer.Visualize(board);
                var score = evaluation.Evaluate(board, out var winner);
                Console.WriteLine(score);
                switch (entry.Winner)
                {
                    case 0:
                        Console.WriteLine("DRAW");
                        break;
                    case 1:
                        Console.WriteLine("WIN");
                        break;
                    case 2:
                        Console.WriteLine("LOSS");
                        break;
                }
            });
            Console.WriteLine(result);
        }

        static void RunTournament()
        {
            var runner = new TournamentRunner();
            runner.Run(() =>
            {
                var eval1 = new Evaluation();
                var solver1 = new Solver(eval1);
                return solver1;
            }, () =>
            {
                var eval2 = new Evaluation();
                //var solver2 = new Solver(eval2, 6);
                //var solver2 = new SolverOld(eval2, 8);
                var solver2 = new Solver(eval2);
                return solver2;
            }, 1);
        }
    }
}
