using System.Diagnostics;
using System.Threading;

namespace ConnectGame.Search
{
    class SearchStopper
    {
        private readonly Stopwatch _stopwatch;

        private CancellationTokenSource _cancellationTokenSource;
        private SearchParameters _parameters;
        private long _minTime;
        private long _maxTime;

        public SearchStopper()
        {
            _stopwatch = new Stopwatch();
        }

        public void NewSearch(SearchParameters parameters, Board board, CancellationToken externalToken)
        {
            _stopwatch.Restart();
            _parameters = parameters;

            var whiteToMove = board.Player == 1;
            var time = whiteToMove ? parameters.WhiteTime : parameters.BlackTime;
            var increment = whiteToMove ? parameters.WhiteTimeIncrement : parameters.BlackTimeIncrement;

            if (parameters.Infinite)
            {
                _minTime = long.MaxValue;
                _maxTime = long.MaxValue;
            }
            else
            {
                var estimatedMovesRemaining = (board.Width * board.Height) - board.History.Count;
                var estimatedOwnMovesRemaining = (estimatedMovesRemaining / 2) + 1;
                var safetyFactor = 0.9;
                _minTime = (int)(time * safetyFactor / estimatedOwnMovesRemaining) + increment;
                _maxTime = _minTime * 3;
                if (_maxTime > time * 0.7)
                {
                    _maxTime = (int)(time * 0.7);
                }
            }

            

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        }

        public bool ShouldStopOnDepthIncrease(int depthReached)
        {
            if (_parameters.MaxDepth.HasValue && depthReached >= _parameters.MaxDepth.Value)
            {
                _cancellationTokenSource.Cancel();
                return true;
            }

            var elapsed = GetSearchedTime();
            if (elapsed >= _minTime)
            {
                _cancellationTokenSource.Cancel();
                return true;
            }

            var cancellationRequested = _cancellationTokenSource.IsCancellationRequested;
            return cancellationRequested;
        }

        public bool ShouldStop()
        {
            var elapsed = GetSearchedTime();
            if (elapsed >= _maxTime)
            {
                _cancellationTokenSource.Cancel();
                return true;
            }

            return _cancellationTokenSource.IsCancellationRequested;
        }

        public bool IsStopped()
        {
            return _cancellationTokenSource.IsCancellationRequested;
        }

        public double GetSearchedTime()
        {
            return _stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
