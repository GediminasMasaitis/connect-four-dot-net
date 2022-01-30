//using System.Collections.Generic;

//namespace ConnectGame.Eval
//{
//    class EvaluationNew : IEvaluation
//    {
//        private readonly int[] _bonuses;
//        private readonly List<IList<Coordinate>> _groups;

//        public EvaluationNew()
//        {
//            _bonuses = new int[]
//            {
//                0,
//                2,
//                18,
//                1000,
//                1000,
//                1000,
//                1000,
//                1000,
//                1000,
//                1000,
//                1000,
//                1000,
//                1000,
//            };

//            var width = 7;
//            var height = 6;

//            _groups = new List<IList<Coordinate>>();

//            for (int row = 0; row < height; row++)
//            {
//                var line = new List<Coordinate>();
//                for (int column = 0; column < width; column++)
//                {
//                    var coordinate = new Coordinate(column, row);
//                    line.Add(coordinate);
//                }
//                _groups.Add(line);
//            }

//            var diagonalStarts = new List<Coordinate>();
//            var antidiagonalStarts = new List<Coordinate>();
//            for (int column = 0; column < width; column++)
//            {
//                var diagonalStart = new Coordinate(column, 0);
//                diagonalStarts.Add(diagonalStart);
//                antidiagonalStarts.Add(diagonalStart);
//            }

//            for (int row = 1; row < height; row++)
//            {
//                var diagonalStart = new Coordinate(0, row);
//                var antidiaginalStart = new Coordinate(width - 1, row);
//                diagonalStarts.Add(diagonalStart);
//                antidiagonalStarts.Add(antidiaginalStart);
//            }

//            var allDiagonalStarts = new List<(IList<Coordinate>, Coordinate)>()
//            {
//                (diagonalStarts, new Coordinate(1, 1)),
//                (antidiagonalStarts, new Coordinate(-1, 1))
//            };

//            foreach (var (starts, offset) in allDiagonalStarts)
//            {
//                foreach (var start in starts)
//                {
//                    var coordinate = start;
//                    var line = new List<Coordinate>();
//                    while (true)
//                    {
//                        var isInMap = IsInMap(width, height, coordinate);
//                        if (!isInMap)
//                        {
//                            break;
//                        }

//                        line.Add(coordinate);
//                        coordinate = coordinate + offset;
//                    }

//                    if (line.Count >= 4)
//                    {
//                        _groups.Add(line);
//                    }
//                }
//            }



//            var a = 123;
//        }

//        private bool IsInMap(int width, int height, Coordinate coordinate)
//        {
//            if (coordinate.Column < 0)
//            {
//                return false;
//            }

//            if (coordinate.Row < 0)
//            {
//                return false;
//            }

//            if (coordinate.Column >= width)
//            {
//                return false;
//            }

//            if (coordinate.Row >= height)
//            {
//                return false;
//            }

//            return true;
//        }

//        public int Evaluate(Board board, out int winner)
//        {
//            // TODO
//            winner = -1;
//            var scores = new int[3];

//            for (int column = 0; column < board.Width; column++)
//            {
//                if (board.Fills[column] == board.Height)
//                {
//                    continue;
//                }

//                if (board.Fills[column] == 0)
//                {
//                    continue;
//                }

//                var currentPlayer = board.Cells[column][board.Fills[column] - 1];
//                var count = 0;
//                for (var row = board.Fills[column] - 2; row >= 0; row--)
//                {
//                    var player = board.Cells[column][row];
//                    if (player != currentPlayer)
//                    {
//                        break;
//                    }

//                    count++;
//                }

//                var bonus = _bonuses[count];
//                scores[currentPlayer] += bonus;
//            }

//            foreach (var group in _groups)
//            {
//                var openEnd = false;
//                var currentPlayer = 0;
//                var count = 0;
//                foreach (var coordinate in group)
//                {
//                    var player = board[coordinate];
//                    if (player == 0)
//                    {
//                        openEnd = true;
//                    }

//                    if (player == currentPlayer)
//                    {
//                        count++;
//                    }
//                    else
//                    {
//                        if (openEnd)
//                        {
//                            var bonus = _bonuses[count];
//                            scores[currentPlayer] += bonus;
//                        }

//                        if (player != 0 && currentPlayer != 0)
//                        {
//                            openEnd = false;
//                        }

//                        currentPlayer = player;
//                        count = 0;
//                    }
//                }

//                if (openEnd)
//                {
//                    var bonus = _bonuses[count];
//                    scores[currentPlayer] += bonus;
//                }
//            }

//            var score = scores[1] - scores[2];
//            return score;
//        }

//        public void ResetState()
//        {
            
//        }
//    }
//}