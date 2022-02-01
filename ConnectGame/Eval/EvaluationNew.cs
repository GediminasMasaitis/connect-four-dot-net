using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConnectGame.Eval
{
    class EvaluationNew : IEvaluation
    {
        private readonly int[] _bonuses;
        private readonly (int[], bool)[] _groups;
        private readonly EvaluationCache _cache;
        //private readonly Evaluation _eval2;

        public EvaluationNew()
        {
            //_eval2 = new Evaluation();
            _cache = new EvaluationCache(1024 * 1024 * 4);

            _bonuses = new int[]
            {
                0,
                2,
                18,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
                1000,
            };

            const int width = Rules.Width;
            const int height = Rules.Height;

            var groups = new List<(IList<int>, bool)>();

            for (int column = 0; column < width; column++)
            {
                var line = new List<int>();
                for (int row = 0; row < height; row++)
                {
                    var cell = column + row * width;
                    line.Add(cell);
                }
                groups.Add((line, false));
            }

            for (int row = 0; row < height; row++)
            {
                var line = new List<int>();
                for (int column = 0; column < width; column++)
                {
                    var cell = column + row * width;
                    line.Add(cell);
                }
                groups.Add((line, true));
            }

            var diagonalStarts = new List<Coordinate>();
            var antidiagonalStarts = new List<Coordinate>();
            for (int column = 0; column < width; column++)
            {
                var diagonalStart = new Coordinate(column, 0);
                diagonalStarts.Add(diagonalStart);
                antidiagonalStarts.Add(diagonalStart);
            }

            for (int row = 1; row < height; row++)
            {
                var diagonalStart = new Coordinate(0, row);
                var antidiaginalStart = new Coordinate(width - 1, row);
                diagonalStarts.Add(diagonalStart);
                antidiagonalStarts.Add(antidiaginalStart);
            }

            var allDiagonalStarts = new List<(IList<Coordinate>, Coordinate)>()
            {
                (diagonalStarts, new Coordinate(1, 1)),
                (antidiagonalStarts, new Coordinate(-1, 1))
            };

            foreach (var (starts, offset) in allDiagonalStarts)
            {
                foreach (var start in starts)
                {
                    var coordinate = start;
                    var line = new List<int>();
                    while (true)
                    {
                        var isInMap = IsInMap(width, height, coordinate);
                        if (!isInMap)
                        {
                            break;
                        }

                        var cell = coordinate.ToCell(width);
                        line.Add(cell);
                        coordinate = coordinate + offset;
                    }

                    if (line.Count >= 4)
                    {
                        groups.Add((line, true));
                    }
                }
            }

            _groups = groups.Select(x =>
            {
                var (group, eval) = x;
                return (group.ToArray(), eval);
            }).ToArray();
        }

        private bool IsInMap(int width, int height, Coordinate coordinate)
        {
            if (coordinate.Column < 0)
            {
                return false;
            }

            if (coordinate.Row < 0)
            {
                return false;
            }

            if (coordinate.Column >= width)
            {
                return false;
            }

            if (coordinate.Row >= height)
            {
                return false;
            }

            return true;
        }

        public int Evaluate(Board board, out int winner)
        {
            //return EvaluateInner(board, out winner);

            if (_cache.TryGet(board.Key, out var entry))
            {
                winner = entry.Winner;
                return entry.Score;
            }

            var score = EvaluateInner(board, out winner);
            //_eval2.Evaluate(board, out var winner2);
            //if (winner != winner2)
            //{
            //    var a = 123;
            //}
            _cache.Set(board.Key, score, winner);
            return score;
        }

        public int EvaluateInner(Board board, out int winner)
        {
            // TODO
            winner = -1;

            var scores = new int[3];

            //for (int column = 0; column < board.Width; column++)
            //{
            //    if (board.Fills[column] == board.Height)
            //    {
            //        continue;
            //    }

            //    if (board.Fills[column] == 0)
            //    {
            //        continue;
            //    }

            //    var topRow = board.Fills[column] - 1;
            //    var currentCell = column + topRow * board.Width;
            //    var currentPlayer = board.Cells[currentCell];
            //    var count = 0;
            //    for (var row = board.Fills[column] - 2; row >= 0; row--)
            //    {
            //        var cell = column + row * board.Width;
            //        var player = board.Cells[cell];
            //        if (player != currentPlayer)
            //        {
            //            break;
            //        }

            //        count++;
            //    }

            //    var bonus = _bonuses[count];
            //    scores[currentPlayer] += bonus;
            //}

            foreach (var (group, doEval) in _groups)
            {
                var openEnd = false;
                var currentPlayer = 0;
                var count = 0;
                foreach (var cell in group)
                {
                    var player = board.Cells[cell];
                    if (player == 0)
                    {
                        openEnd = true;
                    }

                    if (player == currentPlayer)
                    {
                        count++;
                        if (currentPlayer != 0 && count >= 3)
                        {
                            winner = currentPlayer;
                            return 0;
                        }
                    }
                    else
                    {
                        if (openEnd)
                        {
                            var bonus = _bonuses[count];
                            scores[currentPlayer] += bonus;
                        }

                        if (player != 0 && currentPlayer != 0)
                        {
                            openEnd = false;
                        }

                        currentPlayer = player;
                        count = 0;
                    }
                }

                if (openEnd && doEval)
                {
                    var bonus = _bonuses[count];
                    scores[currentPlayer] += bonus;
                }
            }

            var score = scores[1] - scores[2];
            return score;
        }

        public void ResetState()
        {
            _cache.Clear();
        }
    }
}