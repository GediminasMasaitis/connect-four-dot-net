using System.Collections.Generic;

namespace ConnectGame
{
    class MoveOrder
    {
        public void OrderMoves(IList<int> moves, int pvMove)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                var move = moves[i];
                if (move == pvMove)
                {
                    var tempMove = moves[0];
                    moves[0] = move;
                    moves[i] = tempMove;
                }
            }
        }

        //private int GetMoveScore()
    }
}