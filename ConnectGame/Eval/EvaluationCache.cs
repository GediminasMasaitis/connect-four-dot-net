using System;

namespace ConnectGame.Eval
{
    class EvaluationCache
    {
        private readonly ulong _size;
        private readonly EvaluationCacheEntry[] _entries;

        public EvaluationCache(ulong size)
        {
            _size = size;
            _entries = new EvaluationCacheEntry[_size];
        }

        public void Set(ulong key, int score, int winner)
        {
            var index = key % _size;
            var entry = new EvaluationCacheEntry(key, score, winner);
            _entries[index] = entry;
        }

        public bool TryGet(ulong key, out EvaluationCacheEntry entry)
        {
            var index = key % _size;
            entry = _entries[index];

            if (entry.Key != key)
            {
                return false;
            }

            return true;
        }

        public void Clear()
        {
            Array.Clear(_entries, 0, _entries.Length);
        }
    }
}