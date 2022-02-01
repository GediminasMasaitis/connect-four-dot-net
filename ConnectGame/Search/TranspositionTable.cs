using System;
using System.Collections.Generic;

namespace ConnectGame.Search
{
    class TranspositionTable
    {
        private readonly ulong _size;
        private readonly TranspositionTableEntry[] _entries;
        
        public IList<TranspositionTableEntry> PrincipalVariation { get; set; }

        public TranspositionTable(ulong size)
        {
            _size = size;
            _entries = new TranspositionTableEntry[_size];
        }

        public void Set(ulong key, int column, int score, int depth, TranspositionTableFlag flag)
        {
            var index = key % _size;
            var existingEntry = _entries[index];

            var existingExact = existingEntry.Flag == TranspositionTableFlag.Exact;
            var newExact = flag == TranspositionTableFlag.Exact;

            if (existingExact && !newExact)
            {
                return;
            }

            if (!existingExact && newExact)
            {
                var entry1 = new TranspositionTableEntry(key, column, score, depth, flag);
                _entries[index] = entry1;
                return;
            }

            if (existingEntry.Key == key && existingEntry.Depth > depth * 2)
            {
                return;
            }

            var entry2 = new TranspositionTableEntry(key, column, score, depth, flag);
            _entries[index] = entry2;
        }

        public bool TryGet(ulong key, out TranspositionTableEntry entry)
        {
            var index = key % _size;
            entry = _entries[index];

            if (entry.Key != key)
            {
                return false;
            }

            return true;
        }
        private IList<TranspositionTableEntry> GetPrincipalVariation(Board board)
        {
            var entries = new List<TranspositionTableEntry>();
            for (var i = 0; i < 1000; i++)
            {
                var success = TryGet(board.Key, out var entry);
                if (!success)
                {
                    break;
                }

                if (entry.Flag != TranspositionTableFlag.Exact)
                {
                    break;
                }

                entries.Add(entry);
                board.MakeMove(entry.Move);
            }
            return entries;
        }

        public void SavePrincipalVariation(Board board)
        {
            var clone = board.Clone();
            PrincipalVariation = GetPrincipalVariation(clone);
        }

        public void Clear()
        {
            Array.Clear(_entries, 0, _entries.Length);
        }
    }
}
