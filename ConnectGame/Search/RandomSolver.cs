using System;
using System.Threading;
using ConnectGame.Search;

namespace ConnectGame
{
    class RandomSolver : ISolver
    {
        private Random _rng;

        public RandomSolver()
        {
            _rng = new Random(0);
        }

        public int Solve(Board board, SearchParameters searchParameters, CancellationToken cancellationToken)
        {
            while (true)
            {
                var column = _rng.Next(0, board.Width);
                if (board.Fills[column] == board.Cells[column].Length)
                {
                    continue;
                }
                return column;
            }
        }

        public void ResetState()
        {
            
        }
    }
}