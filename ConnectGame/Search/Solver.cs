using System;
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
        private readonly TranspositionTable _table;
        private readonly MoveOrder _order;
        private readonly SearchStopper _stopper;
        private readonly SearchStatistics _stats;

        private const int Inf = 2_000_000;
        private const int Win = 1_000_000;

        public Solver(IEvaluation evaluation, int threads = 1)
        {
            EnableLogs = true;
            _threads = threads;
            _eval = evaluation;
            _table = new TranspositionTable(1024 * 1024 * 8);
            _order = new MoveOrder();
            _stopper = new SearchStopper();
            _stats = new SearchStatistics();
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
            _stats.Reset();
            var maxDepth = searchParameters.MaxDepth ?? 127;
            if (_threads <= 1)
            {
                //var (column, score) = Search(board, _depth, 0, -Inf, Inf);
                //var s = new Stopwatch();
                //s.Start();
                var (column, score) = IterativeDeepen(board, maxDepth);
                //s.Stop();
                //Console.WriteLine(s.Elapsed.TotalMilliseconds);
                return column;
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
                var isValidMove = board.IsValidMove(move);
                if (!isValidMove)
                {
                    return;
                }

                var boardClone = board.Clone();
                boardClone.MakeMove(move);
                var (_, inverseScore) = Search(boardClone, depth - 1, 1, -Inf, Inf, true);
                var score = -inverseScore;
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

        private (int, int) IterativeDeepen(Board board, int maxDepth)
        {
            int savedMove = 0;
            int score = -Inf;
            for (int depth = 2; depth <= maxDepth; depth++)
            {
                int move;
                (move, score) = Search(board, depth, 0, -Inf, Inf, true);

                if (_stopper.IsStopped())
                {
                    break;
                }

                var elapsed = (int)_stopper.GetSearchedTime();
                if (elapsed == 0)
                {
                    elapsed = 1;
                }

                var nps = (_stats.NodesSearched * 1000) / elapsed;

                if (EnableLogs)
                {
                    var clone = board.Clone();
                    var principalVariation = _table.GetPrincipalVariation(clone);
                    var pvBuilder = new StringBuilder();
                    for (var i = 0; i < principalVariation.Count; i++)
                    {
                        var entry = principalVariation[i];
                        pvBuilder.Append(entry.Move);
                        if (i < principalVariation.Count - 1)
                        {
                            pvBuilder.Append(' ');
                        }
                    }

                    var pvStr = pvBuilder.ToString();
                    Console.WriteLine($"info depth {depth} nodes {_stats.NodesSearched} time {elapsed} score pts {score} nps {nps} pv {pvStr}");
                }

                savedMove = move;

                var isStopped = _stopper.ShouldStopOnDepthIncrease(depth);
                if (isStopped)
                {
                    break;
                }
            }

            return (savedMove, score);
        }

        private (int, int) Search(Board board, int depth, int ply, int alpha, int beta, bool isPrincipalVariation)
        {
            // STOP CHECK
            if (depth > 2 && _stopper.ShouldStop())
            {
                return (0, alpha);
            }

            var eval = Eval(board, out var winner);
            _stats.NodesSearched++;

            if (winner != -1)
            {
                if (winner == 0)
                {
                    return (default, 0);
                }

                if (winner == board.Player)
                {
                    return (default, Win - ply);
                }

                return (default, ply - Win);
            }

            if (depth == 0)
            {
                return (default, eval);
            }

            var entryExists = _table.TryGet(board.Key, out var entry);
            var pvMove = entryExists ? entry.Move : -1;
            if (entryExists)
            {
                if (entry.Depth >= depth)
                {
                    switch (entry.Flag)
                    {
                        case TranspositionTableFlag.Exact:
                            return (entry.Move, entry.Score);
                        case TranspositionTableFlag.Alpha:
                            if (entry.Score <= alpha)
                            {
                                return (entry.Move, alpha);
                            }
                            break;
                        case TranspositionTableFlag.Beta:
                            if (entry.Score >= beta)
                            {
                                return (entry.Move, beta);
                            }
                            break;
                    }
                }
            }

            //var moves = new int[board.Width];
            //for (var column = 0; column < board.Width; column++)
            //{
            //    moves[column] = column;
            //}

            var player = board.Player;
            var movesEvaluated = 0;
            var bestColumn = 0;
            var bestScore = -Inf;
            var raisedAlpha = false;
            var betaCutoff = false;
            var moves = new int[] { 3, 2, 4, 1, 5, 0, 6 };

            _order.OrderMoves(moves, pvMove);

            for (var moveIndex = 0; moveIndex < moves.Length; moveIndex++)
            {
                var move = moves[moveIndex];
                var isValidMove = board.IsValidMove(move);
                if (!isValidMove)
                {
                    continue;
                }

                board.MakeMove(move);
                int score;
                if (movesEvaluated > 0)
                {
                    var (_, inverseScore) = Search(board, depth - 1, ply + 1, -alpha - 1, -alpha, false);
                    score = -inverseScore;
                    if (score > alpha)
                    {
                        var (_, inverseReScore) = Search(board, depth - 1, ply + 1, -beta, -alpha, isPrincipalVariation);
                        score = -inverseReScore;
                    }
                }
                else
                {
                    var (_, inverseScore) = Search(board, depth - 1, ply + 1, -beta, -alpha, isPrincipalVariation);
                    score = -inverseScore;
                }

                board.UnmakeMove();
                movesEvaluated++;
                if (score > bestScore)
                {
                    bestColumn = move;
                    bestScore = score;
                    if (score > alpha)
                    {
                        raisedAlpha = true;
                        alpha = score;
                        if (score >= beta)
                        {
                            betaCutoff = true;
                            break;
                        }
                    }
                }
            }

            if (betaCutoff)
            {
                _table.Set(board.Key, bestColumn, bestScore, depth, TranspositionTableFlag.Beta);
                return (bestColumn, beta);
            }

            if (raisedAlpha)
            {
                _table.Set(board.Key, bestColumn, bestScore, depth, TranspositionTableFlag.Exact);
            }
            else
            {
                _table.Set(board.Key, bestColumn, bestScore, depth, TranspositionTableFlag.Alpha);
            }

            return (bestColumn, alpha);
        }

        private void StoreEntry(ulong key, int column, int score, int depth, TranspositionTableFlag flag)
        {
            if (_stopper.ShouldStop())
            {
                return;
            }

            _table.Set(key, column, score, depth, flag);
        }

        public void ResetState()
        {
            _table.Clear();
            _eval.ResetState();
        }
    }
}