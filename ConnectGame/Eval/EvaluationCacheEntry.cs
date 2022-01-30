namespace ConnectGame.Eval
{
    struct EvaluationCacheEntry
    {
        public ulong Key { get; }
        public int Score { get; }
        public int Winner { get; }

        public EvaluationCacheEntry(ulong key, int score, int winner)
        {
            Key = key;
            Score = score;
            Winner = winner;
        }
    }
}