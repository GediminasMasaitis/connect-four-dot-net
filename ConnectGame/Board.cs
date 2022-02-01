using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

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

        public byte[] Cells { get; private set; }
        //public ulong[] Bitboards { get; private set; }
        public IList<int> History { get; private set; }
        public int[] Fills { get; private set; }
        public byte Player { get; private set; }
        public ulong Key { get; private set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;

            Cells = new byte[width * height];
            CellCount = Cells.Length;

            Fills = new int[width];
            History = new List<int>(width * height);
            Player = 1;
            Key = Zobrist.CalculateKey(this);
        }

        public void MakeColumn(int column)
        {
            var row = Fills[column];
            var cell = column + row * Width;
            MakeMove(cell);
        }
        public void MakeMove(int cell)
        {
            History.Add(cell);

            var originalPlayer = Player;
            SwapPlayers();
            Key ^= Zobrist.Player;

            if (cell < 0)
            {
                return;
            }

            var column = cell % Width;
            Fills[column]++;
            Cells[cell] = originalPlayer;
            Key ^= Zobrist.Cells[cell][originalPlayer];
        }

        public void UnmakeMove()
        {
            var cell = History[^1];
            History.RemoveAt(History.Count - 1);

            var originalPlayer = Player;
            SwapPlayers();
            Key ^= Zobrist.Player;

            if (cell < 0)
            {
                return;
            }

            var column = cell % Width;
            Fills[column]--;
            Cells[cell] = 0;
            Key ^= Zobrist.Cells[cell][Player];
        }

        public bool IsValidColumn(int column)
        {
            return Fills[column] < Height;
        }

        public bool IsValidMove(int move)
        {
            return Cells[move] == 0;
        }

        private void SwapPlayers()
        {
            Player = Player == 1 ? (byte)2 : (byte)1;
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