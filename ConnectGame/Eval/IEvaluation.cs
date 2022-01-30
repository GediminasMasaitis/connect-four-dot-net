namespace ConnectGame.Eval
{
    internal interface IEvaluation
    {
        int Evaluate(Board board, out int winner);
        void ResetState();
    }
}