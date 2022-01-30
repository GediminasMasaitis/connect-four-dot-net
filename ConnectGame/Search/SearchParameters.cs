namespace ConnectGame.Search
{
    public class SearchParameters
    {
        public bool Infinite { get; set; }
        public int? MaxDepth { get; set; }

        public long WhiteTime { get; set; }
        public long BlackTime { get; set; }

        public long WhiteTimeIncrement { get; set; }
        public long BlackTimeIncrement { get; set; }

        public SearchParameters()
        {
            Infinite = false;
            MaxDepth = null;
            WhiteTime = 0;
            BlackTime = 0;
            WhiteTimeIncrement = 0;
            BlackTimeIncrement = 0;
        }
    }
}