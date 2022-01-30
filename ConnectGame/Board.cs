using System.Collections.Generic;
using System.Linq;

namespace ConnectGame
{
    class Board
    {
        public int Width { get; }
        public int Height { get; }

        public int[][] Cells { get; private set; }
        public IList<int> History { get; private set; }
        public int[] Fills { get; private set; }
        public int Player { get; private set; }
        public ulong Key { get; private set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new int[width][];
            for (var i = 0; i < width; i++)
            {
                Cells[i] = new int[height];
            }

            Fills = new int[width];
            History = new List<int>(width * height);
            Player = 1;
            Key = Zobrist.CalculateKey(this);
        }

        public int this[Coordinate coordinate]
        {
            get => Cells[coordinate.Column][coordinate.Row];
            set => Cells[coordinate.Column][coordinate.Row] = value;
        }

        public void MakeMove(int column)
        {
            History.Add(column);

            var player = Player;
            SwapPlayers();

            if (column < 0)
            {
                return;
            }

            var row = Fills[column];
            Fills[column]++;
            Cells[column][row] = player;
            Key = Zobrist.CalculateKey(this);
        }

        public void UnmakeMove()
        {
            var column = History[^1];
            History.RemoveAt(History.Count - 1);

            SwapPlayers();
            var player = Player;

            if (column < 0)
            {
                return;
            }

            Fills[column]--;
            var row = Fills[column];
            Cells[column][row] = 0;
            Key = Zobrist.CalculateKey(this);
        }

        public bool IsValidMove(int column)
        {
            return Fills[column] < Height;
        }

        private void SwapPlayers()
        {
            Player = Player == 1 ? 2 : 1;
        }

        //private void AssertFillsCorrect()
        //{
        //    for (int column = 0; column < Width; column++)
        //    {
                
        //    }
        //}

        public Board Clone()
        {
            var board = new Board(Width, Height);
            for (int column = 0; column < Width; column++)
            {
                board.Cells[column] = Cells[column].ToArray();
            }

            board.History = History.ToList();
            board.Fills = Fills.ToArray();
            board.Player = Player;
            board.Key = Key;
            return board;
        }
    }
}