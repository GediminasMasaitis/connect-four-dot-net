using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectGame.Eval;

namespace ConnectGame.Search
{
    class Solver : ISolver
    {
        public bool EnableLogs { get; set; }

        private readonly int _threads;
        private readonly IEvaluation _eval;
        private readonly SearchState _state;
        private readonly MoveOrder _order;
        private readonly SearchStopper _stopper;
        private readonly SearchStatistics _stats;
        //private readonly CellWinDetector _cellWinDetector;

        private const int Inf = 2_000_000;
        private const int Win = 1_000_000;

        public Solver(IEvaluation evaluation, int threads = 1)
        {
            EnableLogs = true;
            _threads = threads;
            _eval = evaluation;
            _state = new SearchState();
            _order = new MoveOrder();
            _stopper = new SearchStopper();
            _stats = new SearchStatistics();
            //_cellWinDetector = new CellWinDetector();
        }

        private int Eval(Board board, out int winner)
        {
            var eval = _eval.Evaluate(board, out winner);
            if (board.Player != 1)
            {
                eval = -eval;
            }

            return eval;
        }

        public int Solve(Board board, SearchParameters searchParameters, CancellationToken cancellationToken)
        {
            _stopper.NewSearch(searchParameters, board, cancellationToken);
            _state.NewSearch();
            _stats.Reset();
            var maxDepth = searchParameters.MaxDepth ?? 127;
            if (_threads <= 1)
            {
                IterativeDeepen(board, maxDepth);
                var bestmove = _state.Table.PrincipalVariation[0].Move;
                var bestColumn = bestmove % board.Width;
                return bestColumn;
            }

            return SolveMultiThreaded(board, maxDepth);
        }

        private int SolveMultiThreaded(Board board, int depth)
        {
            var scores = new int[board.Width];
            var moves = new int[board.Width];
            for (var column = 0; column < board.Width; column++)
            {
                scores[column] = -Inf;
                moves[column] = column;
            }

            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = _threads;
            Parallel.ForEach(moves, options, move =>
            {
                var isValidMove = board.IsValidColumn(move);
                if (!isValidMove)
                {
                    return;
                }

                var boardClone = board.Clone();
                boardClone.MakeMove(move);
                var score = -Search(boardClone, depth - 1, 1, -Inf, Inf, true);
                scores[move] = score;
            });

            var bestMove = 0;
            var bestScore = -Inf;
            for (var move = 0; move < scores.Length; move++)
            {
                var score = scores[move];
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private void IterativeDeepen(Board board, int maxDepth)
        {
            var score = Search(board, 1, 0, -Inf, Inf, true);
            for (int depth = 2; depth <= maxDepth; depth++)
            {
                score = AspirationSearch(board, depth, score);

                if (_stopper.IsStopped())
                {
                    break;
                }

                _state.Table.SavePrincipalVariation(board);
                var elapsed = (int)_stopper.GetSearchedTime();
                if (elapsed == 0)
                {
                    elapsed = 1;
                }

                var nps = (_stats.NodesSearched * 1000) / elapsed;

                if (EnableLogs)
                {
                    var principalVariation = _state.Table.PrincipalVariation;
                    var pvBuilder = new StringBuilder();
                    for (var i = 0; i < principalVariation.Count; i++)
                    {
                        var entry = principalVariation[i];
                        var column = entry.Move % board.Width;
                        pvBuilder.Append(column);
                        if (i < principalVariation.Count - 1)
                        {
                            pvBuilder.Append(' ');
                        }
                    }

                    var pvStr = pvBuilder.ToString();
                    Console.WriteLine($"info depth {depth} nodes {_stats.NodesSearched} time {elapsed} score pts {score} nps {nps} pv {pvStr}");
                }

                var isStopped = _stopper.ShouldStopOnDepthIncrease(depth);
                if (isStopped)
                {
                    break;
                }
            }
        }

        private int AspirationSearch(Board board, int depth, int previousScore)
        {
            return Search(board, depth, previousScore, -Inf, Inf, true);
            const int aspirationWindow = 30;
            var alpha = previousScore - aspirationWindow;
            var beta = previousScore + aspirationWindow;

            var score = Search(board, depth, 0, alpha, beta, true);
            if (score <= alpha || score >= beta)
            {
                score = Search(board, depth, previousScore, -Inf, Inf, true);
            }

            return score;
        }

        private int Search(Board board, int depth, int ply, int alpha, int beta, bool isPrincipalVariation)
        {
            // STOP CHECK
            if (depth > 2 && _stopper.ShouldStop())
            {
                return alpha;
            }

            // MATE DISTANCE PRUNE
            var currentMateScore = Win - ply;
            if (ply != 0)
            {
                if (alpha < -currentMateScore)
                {
                    alpha = -currentMateScore;
                }

                if (beta > currentMateScore - 1)
                {
                    beta = currentMateScore - 1;
                }

                if (alpha >= beta)
                {
                    return alpha;
                }
            }

            var eval = Eval(board, out var winner);
            _stats.NodesSearched++;

            if (winner != -1)
            {
                if (winner == 0)
                {
                    return 0;
                }

                if (winner == board.Player)
                {
                    return currentMateScore;
                }

                return -currentMateScore;
            }

            //var maybeColumns = new int[] { 3, 2, 4, 1, 5, 0, 6 };
            //var columns = maybeColumns.Where(board.IsValidColumn).ToArray();
            //var moves = new int[columns.Length];
            //for (var i = 0; i < columns.Length; i++)
            //{
            //    var column = columns[i];
            //    var row = board.Fills[column];
            //    var move = column + row * board.Width;
            //    moves[i] = move;
            //}

            //if (ply != 0)
            //{
            //    board.MakeMove(-1);
            //    foreach (var move in moves)
            //    {
            //        board.MakeMove(move);
            //        //var enemyEval = Eval(board, out var enemyWinner);
            //        var enemyWinner = _cellWinDetector.GetCellWinner(board, move);
            //        if (enemyWinner != -1)
            //        {
            //            depth++;
            //            board.UnmakeMove();
            //            moves = new int[] { move };
            //            break;
            //        }
            //        board.UnmakeMove();
            //    }
            //    board.UnmakeMove();
            //}

            if (depth == 0)
            {
                return eval;
            }

            //if (!isPrincipalVariation)
            //{
            //    var margin = 24 + depth * 8;
            //    if (eval - margin * depth >= beta)
            //    {
            //        return beta;
            //    }
            //}
            
            var entryExists = _state.Table.TryGet(board.Key, out var entry);
            var pvMove = entryExists ? entry.Move : -1;
            if (entryExists)
            {
                if (ply > 0 && entry.Depth >= depth)
                {
                    switch (entry.Flag)
                    {
                        case TranspositionTableFlag.Exact:
                            return entry.Score;
                        case TranspositionTableFlag.Alpha:
                            if (entry.Score <= alpha)
                            {
                                return alpha;
                            }
                            break;
                        case TranspositionTableFlag.Beta:
                            if (entry.Score >= beta)
                            {
                                return beta;
                            }
                            break;
                    }
                }
            }

            // STATIC EVALUATION PRUNING
            if
            (
                ply > 0
                && depth < 7
                && !isPrincipalVariation
            )
            {
                const int marginPerDepth = 20;
                var margin = marginPerDepth * depth;

                if (eval - margin >= beta)
                {
                    return eval - margin;
                }
            }

            //var moves = new int[board.Width];
            //for (var column = 0; column < board.Width; column++)
            //{
            //    moves[column] = column;
            //}

            var player = board.Player;
            var movesEvaluated = 0;
            var bestMove = 0;
            var bestScore = -Inf;
            var raisedAlpha = false;
            var betaCutoff = false;
            //var columns = new int[] { 3, 2, 4, 1, 5, 0, 6 };

            var maybeColumns = new int[] { 3, 2, 4, 1, 5, 0, 6 };
            var columns = maybeColumns.Where(board.IsValidColumn).ToArray();
            var moves = new int[columns.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                var row = board.Fills[column];
                var move = column + row * board.Width;
                moves[i] = move;
            }

            var scores = _order.GetMoveScores(board, ply, _state, moves, pvMove);
            for (var moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                _order.OrderNextMove(moveIndex, moves, scores);
                var move = moves[moveIndex];
                //var column = columns[]
                //var isValidMove = board.IsValidMove(move);
                //if (!isValidMove)
                //{
                //    continue;
                //}

                board.MakeMove(move);
                int score;
                if (movesEvaluated > 0)
                {
                    score = -Search(board, depth - 1, ply + 1, -alpha - 1, -alpha, false);
                    if (score > alpha)
                    {
                        score = -Search(board, depth - 1, ply + 1, -beta, -alpha, isPrincipalVariation);
                    }
                }
                else
                {
                    score = -Search(board, depth - 1, ply + 1, -beta, -alpha, isPrincipalVariation);
                }

                board.UnmakeMove();
                movesEvaluated++;
                if (score > bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                    if (score > alpha)
                    {
                        //_state.History[board.Player][bestMove] += depth * depth;
                        //if (board.History.Count > 0)
                        //{
                        //    var previousMove = board.History[^1];
                        //    _state.Counters[board.Player][previousMove][bestMove] += depth * depth;
                        //}

                        raisedAlpha = true;
                        alpha = score;
                        if (score >= beta)
                        {

                            //_state.Killers[ply][1] = _state.Killers[ply][0];
                            //_state.Killers[ply][0] = bestMove;
                            betaCutoff = true;
                            break;
                        }
                    }
                }
            }

            if (betaCutoff)
            {
                StoreEntry(board.Key, bestMove, bestScore, depth, TranspositionTableFlag.Beta);
                return beta;
            }

            if (raisedAlpha)
            {
                StoreEntry(board.Key, bestMove, bestScore, depth, TranspositionTableFlag.Exact);
            }
            else
            {
                StoreEntry(board.Key, bestMove, bestScore, depth, TranspositionTableFlag.Alpha);
            }

            return alpha;
        }

        private void StoreEntry(ulong key, int column, int score, int depth, TranspositionTableFlag flag)
        {
            if (_stopper.ShouldStop())
            {
                return;
            }

            _state.Table.Set(key, column, score, depth, flag);
        }

        public void ResetState()
        {
            _state.Clear();
            _eval.ResetState();
        }
    }
}