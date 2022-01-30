using System;
using System.Text;
using ConnectGame.Eval;

namespace ConnectGame
{
    class BoardVisualizer
    {
        private readonly IEvaluation _evaluation;

        public BoardVisualizer(IEvaluation evaluation)
        {
            _evaluation = evaluation;
        }

        public string Visualize(Board board, bool evaluate = true)
        {
            var builder = new StringBuilder();

            for (var row = board.Height - 1; row >= 0; row--)
            {
                for (var column = 0; column < board.Width; column++)
                {
                    var cell = column + row * board.Width;
                    var player = board.Cells[cell];
                    switch (player)
                    {
                        case 0:
                            builder.Append('.');
                            break;
                        case 1:
                            builder.Append('X');
                            break;
                        case 2:
                            builder.Append('O');
                            break;
                        default:
                            throw new Exception("Unknown player");
                    }
                }

                builder.AppendLine();
            }

            builder.AppendLine($"Player to move: {board.Player}");
            if (evaluate)
            {
                var staticEval = _evaluation.Evaluate(board, out var winner);
                builder.AppendLine($"Static eval: {staticEval}");
                builder.AppendLine($"Winner: {winner}");
            }

            return builder.ToString();
        }
    }
}