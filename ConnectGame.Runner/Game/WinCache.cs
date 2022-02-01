using System.Collections.Generic;

namespace ConnectGame.Runner.Game
{
    class WinCache
    {
        private readonly Coordinate[][][][] _cache;

        public WinCache()
        {
            const int width = Rules.Width;
            const int height = Rules.Height;

            var offsets = new Coordinate[]
            {
                new Coordinate(1, 0),
                new Coordinate(0, 1),
                new Coordinate(1, 1),
                new Coordinate(1, -1),
            };

            _cache = new Coordinate[width][][][];
            for (int column = 0; column < width; column++)
            {
                _cache[column] = new Coordinate[height][][];
                for (int row = 0; row < height; row++)
                {
                    var coordinate = new Coordinate(column, row);
                    var offsetList = new List<Coordinate[]>();
                    for (var offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        var target = coordinate;
                        var offset = offsets[offsetIndex];
                        var neighbors = new List<Coordinate>();
                        for (int i = 0; i < 3; i++)
                        {
                            target += offset;
                            var isInMap = IsInMap(width, height, target);
                            if (!isInMap)
                            {
                                break;
                            }

                            neighbors.Add(target);
                        }

                        if (neighbors.Count == 3)
                        {
                            offsetList.Add(neighbors.ToArray());
                        }

                        //if (neighbors.Count > 0)
                        //{
                        //    offsetList.Add(neighbors.ToArray());
                        //}
                    }
                    _cache[column][row] = offsetList.ToArray();
                }
            }
        }

        public Coordinate[][] this[Coordinate coordinate] => _cache[coordinate.Column][coordinate.Row];

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
    }
}