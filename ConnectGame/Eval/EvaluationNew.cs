using System.Collections.Generic;

namespace ConnectGame.Eval
{
    class EvaluationNew : IEvaluation
    {
        private readonly int[] _bonuses;
        private readonly List<IList<Coordinate>> _groups;
        private readonly EvaluationCache _cache;

        public EvaluationNew()
        {
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

            var width = 7;
            var height = 6;

            _groups = new List<IList<Coordinate>>();

            for (int row = 0; row < height; row++)
            {
                var line = new List<Coordinate>();
                for (int column = 0; column < width; column++)
                {
                    var coordinate = new Coordinate(column, row);
                    line.Add(coordinate);
                }
                _groups.Add(line);
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
                    var line = new List<Coordinate>();
                    while (true)
                    {
                        var isInMap = IsInMap(width, height, coordinate);
                        if (!isInMap)
                        {
                            break;
                        }

                        line.Add(coordinate);
                        coordinate = coordinate + offset;
                    }

                    if (line.Count >= 4)
                    {
                        _groups.Add(line);
                    }
                }
            }
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
            if (_cache.TryGet(board.Key, out var entry))
            {
                winner = entry.Winner;
                return entry.Score;
            }

            var score = EvaluateInner(board, out winner);
            _cache.Set(board.Key, score, winner);
            return score;
        }

        public int EvaluateInner(Board board, out int winner)
        {
            // TODO
            winner = -1;

            var scores = new int[3];

            for (int column = 0; column < board.Width; column++)
            {
                if (board.Fills[column] == board.Height)
                {
                    continue;
                }

                if (board.Fills[column] == 0)
                {
                    continue;
                }

                var topRow = board.Fills[column] - 1;
                var currentCell = column + topRow * board.Width;
                var currentPlayer = board.Cells[currentCell];
                var count = 0;
                for (var row = board.Fills[column] - 2; row >= 0; row--)
                {
                    var cell = column + row * board.Width;
                    var player = board.Cells[cell];
                    if (player != currentPlayer)
                    {
                        break;
                    }

                    count++;
                }

                var bonus = _bonuses[count];
                scores[currentPlayer] += bonus;
            }

            foreach (var group in _groups)
            {
                var openEnd = false;
                var currentPlayer = 0;
                var count = 0;
                foreach (var coordinate in group)
                {
                    var cell = coordinate.ToCell(board.Width);
                    var player = board.Cells[cell];
                    if (player == 0)
                    {
                        openEnd = true;
                    }

                    if (player == currentPlayer)
                    {
                        count++;
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

                if (openEnd)
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