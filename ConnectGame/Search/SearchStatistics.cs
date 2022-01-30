namespace ConnectGame.Search
{
    class SearchStatistics
    {
        public long NodesSearched { get; set; }

        public void Reset()
        {
            NodesSearched = 0;
        }
    }
}