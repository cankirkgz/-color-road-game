using System.Collections.Generic;
using RenkYolu.Core;
using RenkYolu.Grid;

namespace RenkYolu.Campaign
{
    public sealed class RoadSolution
    {
        private readonly long[] bestByLength;

        internal RoadSolution(long maximum, int[] bestRoute, int[] shortRoute, long[] bestByLength, ulong winningTiles)
        {
            Maximum = maximum;
            BestRoute = System.Array.AsReadOnly(bestRoute);
            ShortRoute = System.Array.AsReadOnly(shortRoute);
            this.bestByLength = bestByLength;
            WinningTiles = winningTiles;
        }

        public long Maximum { get; }
        public IReadOnlyList<int> BestRoute { get; }
        public IReadOnlyList<int> ShortRoute { get; }
        internal ulong WinningTiles { get; }
        public bool IsSolvable => BestRoute.Count > 0;

        public long BestWithin(int longest)
        {
            long best = 0;
            for (int i = 1; i <= longest && i < bestByLength.Length; i++)
                best = System.Math.Max(best, bestByLength[i]);
            return best;
        }
    }

    public static class RoadSolver
    {
        public static RoadSolution Solve(RoadBoard board)
        {
            var route = new int[board.Count];
            int[] best = System.Array.Empty<int>();
            int[] shortest = System.Array.Empty<int>();
            long shortScore = long.MinValue;
            long maximum = long.MinValue;
            var bestByLength = new long[board.Count + 1];
            ulong winningTiles = 0;

            Visit(RoadBoard.Start, RoadRules.Bit(RoadBoard.Start), 0, 0, -1, 0);
            return new RoadSolution(best.Length > 0 ? maximum : 0, best, shortest, bestByLength, winningTiles);

            // While a launcher is pending we score the drawn detour in order.
            // It becomes a legal walk only when the detour reaches its forced landing.
            void Visit(int cell, ulong used, int depth, long score, int pendingLanding, int skipped)
            {
                route[depth] = cell;
                if (pendingLanding == cell && skipped > 0)
                    score += GameBalance.LauncherShortcutBonus;

                score = RoadRules.ScoreTile(board, cell, score);

                if (pendingLanding >= 0)
                {
                    if (cell == pendingLanding)
                    {
                        pendingLanding = -1;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                if (pendingLanding < 0)
                {
                    if (board.CanFinish(cell))
                        Record(score, depth + 1, used);

                    if (board[cell].Kind == TileSpecialType.Launcher)
                    {
                        int landing = board.LauncherLanding(cell);
                        if (landing < 0 || !board[landing].Walkable || (used & RoadRules.Bit(landing)) != 0)
                            return;

                        RecordForcedFinish(landing, used, depth + 1, score);

                        pendingLanding = landing;
                        skipped = 0;
                    }
                }

                IReadOnlyList<int> neighbours = board.Neighbours(cell);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    int next = neighbours[i];
                    ulong bit = RoadRules.Bit(next);
                    if ((used & bit) == 0)
                        Visit(next, used | bit, depth + 1, score, pendingLanding, skipped);
                }
            }

            void RecordForcedFinish(int cell, ulong used, int length, long score)
            {
                while (cell >= 0 && board[cell].Walkable && (used & RoadRules.Bit(cell)) == 0)
                {
                    used |= RoadRules.Bit(cell);
                    score = RoadRules.ScoreTile(board, cell, score);
                    if (board.CanFinish(cell))
                        Record(score, length, used);
                    if (board[cell].Kind != TileSpecialType.Launcher)
                        return;

                    cell = board.LauncherLanding(cell);
                }
            }

            void Record(long score, int length, ulong used)
            {
                winningTiles |= used;
                bestByLength[length] = System.Math.Max(bestByLength[length], score);
                if (score > maximum || (score == maximum && length < best.Length))
                {
                    maximum = score;
                    best = CopyRoute(length);
                }

                if (score > 0 && (shortest.Length == 0 || length < shortest.Length ||
                    (length == shortest.Length && score > shortScore)))
                {
                    shortScore = score;
                    shortest = CopyRoute(length);
                }
            }

            int[] CopyRoute(int length)
            {
                var result = new int[length];
                System.Array.Copy(route, result, length);
                return result;
            }
        }
    }
}
