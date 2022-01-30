namespace ConnectGame.Search
{
    struct TranspositionTableEntry
    {
        public ulong Key { get; }
        public int Move { get; }
        public int Score { get; }
        public int Depth { get; }
        public TranspositionTableFlag Flag { get; }

        public TranspositionTableEntry(ulong key, int move, int score, int depth, TranspositionTableFlag flag)
        {
            Key = key;
            Move = move;
            Score = score;
            Depth = depth;
            Flag = flag;
        }
    }
}