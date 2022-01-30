using System.Collections.Generic;

namespace ConnectGame
{
    class WinCache
    {
        private readonly int[][][] _cache;

        public WinCache()
        {
            const int width = 7;
            const int height = 6;
            const int cellCount = width * height;

            var offsets = new Coordinate[]
            {
                new Coordinate(1, 0),
                new Coordinate(0, 1),
                new Coordinate(1, 1),
                new Coordinate(1, -1),
            };

            _cache = new int[cellCount][][];
            for (int column = 0; column < width; column++)
            {
                for (int row = 0; row < height; row++)
                {
                    var cell = column + row * width;
                    var coordinate = new Coordinate(column, row);
                    var offsetList = new List<int[]>();
                    for (var offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        var target = coordinate;
                        var offset = offsets[offsetIndex];
                        var neighbors = new List<int>();
                        for (int i = 0; i < 3; i++)
                        {
                            target += offset;
                            var isInMap = IsInMap(width, height, target);
                            if (!isInMap)
                            {
                                break;
                            }

                            var targetCell = target.ToCell(width);
                            neighbors.Add(targetCell);
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
                    _cache[cell] = offsetList.ToArray();
                }
            }
        }

        public int[][] this[int cell] => _cache[cell];

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