using System.Threading;

namespace ConnectGame
{
    class WinDetector
    {
        private readonly int _winCondition;
        private readonly WinCache _cache;

        public WinDetector(int winCondition = 4)
        {
            _winCondition = winCondition;
            _cache = new WinCache();
        }

        public int? GetWinner(Board board)
        {
            var anyEmpty = false;
            for (int column = 0; column < board.Width; column++)
            {
                for (int row = 0; row < board.Height; row++)
                {
                    var coordinate = new Coordinate(column, row);
                    var player = board[coordinate];
                    if (player == 0)
                    {
                        anyEmpty = true;
                        continue;
                    }

                    var groups = _cache[coordinate];
                    foreach (var group in groups)
                    {
                        var count = 0;
                        var win = true;
                        //for (var i = 0; i < _winCondition - 1; i++)
                        foreach (var target in group)
                        {
                            if (board[target] != player)
                            {
                                win = false;
                                break;
                            }
                        }

                        if (win)
                        {
                            return player;
                        }
                    }
                }
            }

            if (anyEmpty)
            {
                return null;
            }

            return 0;
        }
    }
}