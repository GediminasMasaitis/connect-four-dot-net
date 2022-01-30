using System;
using System.Collections.Generic;
using System.Linq;

namespace ConnectGame
{
    class Masks
    {
        public const ulong All = 0x3FFFFFFFFFFUL;
    }

    class Board
    {
        public int Width { get; }
        public int Height { get; }
        public int CellCount { get; }

        public int[] Cells { get; private set; }
        public ulong[] Bitboards { get; private set; }
        public IList<int> History { get; private set; }
        public int[] Fills { get; private set; }
        public int Player { get; private set; }
        public ulong Key { get; private set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;

            Cells = new int[width * height];
            CellCount = Cells.Length;

            Fills = new int[width];
            History = new List<int>(width * height);
            Player = 1;
            Key = Zobrist.CalculateKey(this);
        }

        public void MakeMove(int column)
        {
            History.Add(column);

            var originalPlayer = Player;
            SwapPlayers();
            Key ^= Zobrist.Turns[originalPlayer];
            Key ^= Zobrist.Turns[Player];

            if (column < 0)
            {
                return;
            }

            var row = Fills[column];
            var cell = column + row * Width;
            Fills[column]++;
            Cells[cell] = originalPlayer;

            Key ^= Zobrist.Cells[cell][0];
            Key ^= Zobrist.Cells[cell][originalPlayer];
        }

        public void UnmakeMove()
        {
            var column = History[^1];
            History.RemoveAt(History.Count - 1);

            var originalPlayer = Player;
            SwapPlayers();

            Key ^= Zobrist.Turns[originalPlayer];
            Key ^= Zobrist.Turns[Player];

            if (column < 0)
            {
                return;
            }

            Fills[column]--;
            var row = Fills[column];
            var cell = column + row * Width;
            Cells[cell] = 0;

            Key ^= Zobrist.Cells[cell][0];
            Key ^= Zobrist.Cells[cell][Player];
        }

        public bool IsValidMove(int column)
        {
            return Fills[column] < Height;
        }

        private void SwapPlayers()
        {
            Player = Player == 1 ? 2 : 1;
        }

        public Board Clone()
        {
            var board = new Board(Width, Height);
            board.Cells = Cells.ToArray();
            board.History = History.ToList();
            board.Fills = Fills.ToArray();
            board.Player = Player;
            board.Key = Key;
            return board;
        }
    }
}