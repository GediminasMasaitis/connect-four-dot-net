using System.Collections.Generic;
using System.Net.WebSockets;

namespace ConnectGame.Search
{
    class MoveOrder
    {
        //public void OrderMoves(Board board, SearchState state, int[] moves, int pvMove)
        //{
        //    var scores = new int[moves.Length];

        //    for (int i = 0; i < moves.Length; i++)
        //    {
        //        var move = moves[i];
        //        if (move == pvMove)
        //        {
        //            var tempMove = moves[0];
        //            moves[0] = move;
        //            moves[i] = tempMove;
        //        }
        //    }
        //}

        public void OrderNextMove(int currentIndex, int[] moves, int[] scores)
        {
            var bestScore = int.MinValue;
            var bestScoreIndex = -1;

            for (var moveIndex = currentIndex; moveIndex < moves.Length; moveIndex++)
            {
                var score = scores[moveIndex];

                if (score > bestScore)
                {
                    bestScore = score;
                    bestScoreIndex = moveIndex;
                }
            }

            (moves[currentIndex], moves[bestScoreIndex]) = (moves[bestScoreIndex], moves[currentIndex]);
            (scores[currentIndex], scores[bestScoreIndex]) = (scores[bestScoreIndex], scores[currentIndex]);
        }

        private int GetMoveScore(Board board, int move, SearchState state, int pvMove)
        {
            if (move == pvMove)
            {
                return 200_000_000;
            }

            //return state.History[board.Player][move];
            return 0;
        }


        public int[] GetMoveScores(Board board, SearchState state, int[] moves, int pvMove)
        {
            var scores = new int[moves.Length];

            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                var move = moves[moveIndex];
                scores[moveIndex] = GetMoveScore(board, move, state, pvMove);
            }

            return scores;
        }
    }
}