using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectGame
{
    class Zobrist
    {
        public static ulong[][] Cells { get; set; }
        public static ulong[] Turns { get; set; }

        public static void Init(int width, int height)
        {
            const int playerCount = 3;

            var rng = new Random(0);
            var cellCount = width * height;
            Cells = new ulong[cellCount][];
            for (int cell = 0; cell < cellCount; cell++)
            {
                Cells[cell] = new ulong[playerCount];
                for (int player = 0; player < playerCount; player++)
                {
                    Cells[cell][player] = NextKey(rng);
                }
            }

            Turns = new ulong[playerCount];
            for (int player = 0; player < playerCount; player++)
            {
                Turns[player] = NextKey(rng);
            }
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
            for (int cell = 0; cell < board.CellCount; cell++)
            {
                var player = board.Cells[cell];
                key ^= Cells[cell][player];
            }

            key ^= Turns[board.Player];
            return key;
        }
    }
}
