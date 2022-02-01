using System;

namespace ConnectGame
{
    class Zobrist
    {
        public static ulong Initial { get; set; }
        public static ulong[][] Cells { get; set; }
        public static ulong Player { get; set; }

        public static void Init(int width, int height)
        {
            const int playerCount = 3;

            var rng = new Random(0);
            Initial = NextKey(rng);

            var cellCount = width * height;
            Cells = new ulong[cellCount][];
            for (int cell = 0; cell < cellCount; cell++)
            {
                Cells[cell] = new ulong[playerCount];
                for (int player = 1; player < playerCount; player++)
                {
                    Cells[cell][player] = NextKey(rng);
                }
            }

            Player = NextKey(rng);
        }

        private static ulong NextKey(Random rng)
        {
            var bytes = new byte[8];
            rng.NextBytes(bytes);
            var num = BitConverter.ToUInt64(bytes, 0);
            return num;
        }

        public static ulong CalculateKey(Board board)
        {
            var key = 0UL;
            key ^= Initial;

            for (int cell = 0; cell < board.CellCount; cell++)
            {
                var player = board.Cells[cell];
                key ^= Cells[cell][player];
            }

            if (board.Player == 2)
            {
                key ^= Player;
            }

            return key;
        }
    }
}
