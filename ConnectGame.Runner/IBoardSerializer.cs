using ConnectGame.Runner.Game;

namespace ConnectGame.Runner
{
    internal interface IBoardSerializer
    {
        string Serialize(Board board);
    }
}