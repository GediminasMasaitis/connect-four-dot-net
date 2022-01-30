using System.Text;
using ConnectGame.Runner.Game;

namespace ConnectGame.Runner
{
    class BoardSerializer : IBoardSerializer
    {
        public string SerializeMn(Board board, StringBuilder builder)
        {
            builder.Append("mn ");
            for (int column = 0; column < board.Width; column++)
            {
                for (int row = 0; row < board.Fills[column]; row++)
                {
                    builder.Append(board.Cells[column][row]);
                }

                builder.Append("_");
            }

            var boardString = builder.ToString();
            return boardString;
        }

        public string SerializeMoves(Board board, StringBuilder builder)
        {
            builder.Append("moves ");
            for (var moveIndex = 0; moveIndex < board.History.Count; moveIndex++)
            {
                var move = board.History[moveIndex];
                builder.Append(move);
                if (moveIndex < board.History.Count - 1)
                {
                    builder.Append('_');
                }
            }

            var boardString = builder.ToString();
            return boardString;
        }

        public string Serialize(Board board)
        {
            var builder = new StringBuilder();
            builder.Append("position startpos");
            if (board.History.Count == 0)
            {
                return builder.ToString();
            }

            builder.Append(' ');
            SerializeMoves(board, builder);
            return builder.ToString();
        }
    }
}