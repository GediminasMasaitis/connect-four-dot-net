using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectGame
{
    class Zobrist
    {
        private static ulong[][][] Cells { get; set; }
        private static ulong[] Turns { get; set; }

        public static void Init(int width, int height)
        {
            const int playerCount = 3;

            var rng = new Random(0);
            Cells = new ulong[width][][];
            for (int column = 0; column < width; column++)
            {
                Cells[column] = new ulong[height][];
                for (int row = 0; row < height; row++)
                {
                    Cells[column][row] = new ulong[playerCount];
                    for (int player = 0; player < playerCount; player++)
                    {
                        Cells[column][row][player] = NextKey(rng);
                    }
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
            for (int column = 0; column < board.Width; column++)
            {
                for (int row = 0; row < board.Height; row++)
                {
                    var player = board.Cells[column][row];
                    key ^= Cells[column][row][player];
                }
            }

            key ^= Turns[board.Player];

            return key;
        }
    }
}
