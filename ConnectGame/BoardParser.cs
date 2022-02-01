using System;

namespace ConnectGame
{
    class BoardParser
    {
        public Board Parse(string str)
        {
            var words = str.Split(" ");
            if (words.Length < 2)
            {
                Console.WriteLine("Incorrect position format");
                return null;
            }

            if (words[1] != "startpos")
            {
                Console.WriteLine("Non-startpos not supported");
                return null;
            }

            if (words.Length < 3)
            {
                var startBoard = new Board(Rules.Width, Rules.Height);
                return startBoard;
            }

            if (words[2] != "moves")
            {
                Console.WriteLine("Non-moves not supported");
                return null;
            }

            if (words.Length < 4)
            {
                Console.WriteLine("Moves not provided");
                return null;
            }

            var board = ParseMoves(words[3]);
            return board;
        }

        private Board ParseMoves(string boardStr)
        {
            var board = new Board(Rules.Width, Rules.Height);
            var moveStrs = boardStr.Split("_");
            foreach (var moveStr in moveStrs)
            {
                var column = int.Parse(moveStr);
                var move = column + board.Fills[column] * board.Width;
                board.MakeMove(move);
            }

            return board;
        }
    }
}