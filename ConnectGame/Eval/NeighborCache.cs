using System.Collections.Generic;

namespace ConnectGame
{
    class NeighborCache
    {
        private readonly Coordinate[][][][][] _cache;

        public NeighborCache()
        {
            const int width = 7;
            const int height = 6;

            var offsetGroups = new Coordinate[][]
            {
                new []{ new Coordinate(-1, -1), new Coordinate(1, 1)},
                new []{ new Coordinate(-1, 1), new Coordinate(1, -1)},
                new[] { new Coordinate(-1, 0), new Coordinate(1, 0) },
                new[] { new Coordinate(0, -1), new Coordinate(0, 1) },
            };

            _cache = new Coordinate[width][][][][];
            for (int column = 0; column < width; column++)
            {
                _cache[column] = new Coordinate[height][][][];
                for (int row = 0; row < height; row++)
                {
                    var coordinate = new Coordinate(column, row);
                    //_cache[column][row] = new Coordinate[offsetGroups.Length][][];
                    var coordinateEntry = new List<Coordinate[][]>();
                    for (var groupIndex = 0; groupIndex < offsetGroups.Length; groupIndex++)
                    {
                        var group = offsetGroups[groupIndex];
                        var offsetList = new List<Coordinate[]>();
                        //_cache[column][row][groupIndex] = new Coordinate[2][];
                        for (var offsetIndex = 0; offsetIndex < group.Length; offsetIndex++)
                        {
                            var target = coordinate;
                            var offset = group[offsetIndex];
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

                            if (neighbors.Count > 0)
                            {
                                offsetList.Add(neighbors.ToArray());
                            }
                            //_cache[column][row][groupIndex][offsetIndex] = neighbors.ToArray();
                        }
                        if (offsetList.Count > 0)
                        {
                            coordinateEntry.Add(offsetList.ToArray());
                        }
                    }
                    _cache[column][row] = coordinateEntry.ToArray();
                }
            }
        }

        public Coordinate[][][] this[Coordinate coordinate] => _cache[coordinate.Column][coordinate.Row];

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