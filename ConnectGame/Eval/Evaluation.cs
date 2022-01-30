﻿using System;

namespace ConnectGame.Eval
{
    class Evaluation : IEvaluation
    {
        private readonly NeighborCache _neighbors;
        private readonly EvaluationCache _cache;
        private readonly WinDetector _detector;

        public Evaluation()
        {
            _neighbors = new NeighborCache();
            _cache = new EvaluationCache(1024 * 1024 * 4);
            //_detector = new WinDetector();
        }

        public int Evaluate(Board board, out int winner)
        {
            if (_cache.TryGet(board.Key, out var entry))
            {
                winner = entry.Winner;
                return entry.Score;
            }

            var score = EvaluateInner(board, out winner);
            _cache.Set(board.Key, score, winner);
            return score;
        }

        private int EvaluateInner(Board board, out int winner)
        {
            var scores = new int[3];
            winner = -1;
            var full = true;
            for (var column = 0; column < board.Width; column++)
            {
                for (var row = 0; row < board.Fills[column]; row++)
                {
                    //var coordinate = new Coordinate(column, row);
                    var cell = column + row * board.Width;
                    var player = board.Cells[cell];
                    var coordinateScore = EvaluateCoordinate(board, player, cell, out var win);
                    if (win)
                    {
                        winner = player;
                        return 0;
                    }
                    scores[player] += coordinateScore;
                }

                if (board.Fills[column] < board.Height)
                {
                    full = false;
                }
            }

            if (full)
            {
                winner = 0;
            }

            //scores[board.Player] += 10;

            var score = scores[1] - scores[2];
            return score;
        }

        private int EvaluateCoordinate(Board board, int player, int cell, out bool win)
        {
            if (player == 0)
            {
                throw new Exception();
            }

            var score = 0;
            var neighborGroups = _neighbors[cell];
            win = false;
            foreach (var group in neighborGroups)
            {
                var groupScore = EvaluateGroup(board, player, group, out var groupWin);
                if (groupWin)
                {
                    win = true;
                    break;
                }
                score += groupScore;
            }

            return score;
        }

        private int EvaluateGroup(Board board, int player, int[][] group, out bool win)
        {
            win = false;
            var currentCount = 0;
            var openEnds = 0;
            var beyondGap = 0;
            foreach (var direction in group)
            {
                for (var i = 0; i < direction.Length; i++)
                {
                    var neighbor = direction[i];
                    var targetPlayer = board.Cells[neighbor];
                    if (targetPlayer != player)
                    {
                        if (targetPlayer == 0)
                        {
                            for (int j = i + 1; j < direction.Length; j++)
                            {
                                var beyondGapCell = direction[j];
                                var beyondGapPlayer = board.Cells[beyondGapCell];
                                if (beyondGapPlayer != player)
                                {
                                    if (beyondGapPlayer == 0)
                                    {
                                        openEnds++;
                                    }
                                    break;
                                }

                                beyondGap++;
                            }

                            openEnds++;
                        }

                        break;
                    }

                    currentCount++;
                }
            }

            if (currentCount > 2)
            {
                win = true;
                currentCount = 1000;
            }

            var potentialCount = currentCount + beyondGap;
            if (potentialCount >= 2)
            {
                potentialCount *= 3;
            }

            potentialCount *= openEnds;
            return potentialCount;
        }

        public void ResetState()
        {
            _cache.Clear();
        }
    }
}
