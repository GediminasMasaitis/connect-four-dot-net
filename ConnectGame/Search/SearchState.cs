using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectGame.Search
{
    class SearchState
    {
        public TranspositionTable Table { get; }
        public int[][] History { get; }

        public SearchState()
        {
            const int width = 7;
            const int height = 6;
            const int cellCount = width * height;

            Table = new TranspositionTable(1024 * 1024 * 8);
            History = new int[3][];
            for (int i = 0; i < History.Length; i++)
            {
                History[i] = new int[cellCount];
            }
        }

        public void Clear()
        {
            Table.Clear();
            foreach (var historyEntry in History)
            {
                Array.Clear(historyEntry, 0, historyEntry.Length);
            }
        }

        public void NewSearch()
        {
            foreach (var historyEntry in History)
            {
                for (var index = 0; index < historyEntry.Length; index++)
                {
                    historyEntry[index] /= 8;
                }
            }
        }
    }
}
