//#define ENABLE_HISTORY
//#define ENABLE_COUNTERS

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

#if ENABLE_HISTORY
        public int[][] History { get; }
#endif

#if ENABLE_COUNTERS
        public int[][][] Counters { get; }
#endif

        //public int[][] Killers { get; }

        public SearchState()
        {
            const int width = Rules.Width;
            const int height = Rules.Height;
            const int cellCount = width * height;

            Table = new TranspositionTable(1024 * 1024 * 8);

#if ENABLE_HISTORY
            History = new int[3][];
            for (int i = 0; i < History.Length; i++)
            {
                History[i] = new int[cellCount];
            }
#endif

#if ENABLE_COUNTERS
            Counters = new int[3][][];
            for (int i = 0; i < Counters.Length; i++)
            {
                Counters[i] = new int[cellCount][];
                for (int j = 0; j < Counters[i].Length; j++)
                {
                    Counters[i][j] = new int[cellCount];
                }
            }
#endif

            //Killers = new int[127][];
            //for (var i = 0; i < Killers.Length; i++)
            //{
            //    Killers[i] = new int[2];
            //}
        }

        public void Clear()
        {
            Table.Clear();

#if ENABLE_HISTORY
            foreach (var historyEntry in History)
            {
                Array.Clear(historyEntry, 0, historyEntry.Length);
            }
#endif

#if ENABLE_COUNTERS
            foreach (var counterPlayer in Counters)
            {
                foreach (var counterEntry in counterPlayer)
                {
                    Array.Clear(counterEntry, 0, counterEntry.Length);
                }
            }
#endif

            //foreach (var killer in Killers)
            //{
            //    Array.Clear(killer, 0, killer.Length);
            //}
        }

        public void NewSearch()
        {
#if ENABLE_HISTORY
            foreach (var historyEntry in History)
            {
                for (var index = 0; index < historyEntry.Length; index++)
                {
                    historyEntry[index] /= 8;
                }
            }
#endif

#if ENABLE_COUNTERS
            foreach (var counterPlayer in Counters)
            {
                foreach (var counterEntry in counterPlayer)
                {
                    for (var index = 0; index < counterEntry.Length; index++)
                    {
                        counterEntry[index] /= 8;
                    }
                }
            }
#endif

            //foreach (var killer in Killers)
            //{
            //    for (int index = 0; index < killer.Length; index++)
            //    {
            //        killer[index] = 0;
            //    }
            //}
        }
    }
}
