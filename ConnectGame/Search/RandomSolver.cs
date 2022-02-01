using System;
using System.Threading;

namespace ConnectGame.Search
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
                if (board.Fills[column] == board.Height)
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