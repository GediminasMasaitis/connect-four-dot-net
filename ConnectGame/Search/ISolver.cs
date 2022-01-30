using System.Threading;

namespace ConnectGame.Search
{
    interface ISolver
    {
        int Solve(Board board, SearchParameters searchParameters, CancellationToken cancellationToken);
        void ResetState();
    }
}