using System.Collections.Generic;
using RenkYolu.Core;
using RenkYolu.Grid;

namespace RenkYolu.Campaign
{
    public enum RoadFailure
    {
        None,
        InvalidPath,
        WrongColor,
        ObstacleFinish,
        LauncherOffBoard,
        LauncherBlocked,
        LauncherMissedPath,
        LauncherRevisit
    }

    public enum RoadStepKind
    {
        Hop,
        Launch,
        Skipped,
        ShortcutBonus,
        Fall
    }

    public readonly struct RoadStep
    {
        public readonly RoadStepKind Kind;
        public readonly int Cell;
        public readonly long Score;
        public readonly int Combo;
        public readonly bool ColorBonus;
        public readonly bool PickedShard;

        public RoadStep(RoadStepKind kind, int cell, long score, int combo, bool colorBonus = false, bool pickedShard = false)
        {
            Kind = kind;
            Cell = cell;
            Score = score;
            Combo = combo;
            ColorBonus = colorBonus;
            PickedShard = pickedShard;
        }
    }

    public sealed class RoadRun
    {
        internal RoadRun(List<RoadStep> steps, RoadFailure failure, long score, long shardMask)
        {
            Steps = steps.AsReadOnly();
            Failure = failure;
            Score = score;
            ShardMask = shardMask;
        }

        public IReadOnlyList<RoadStep> Steps { get; }
        public RoadFailure Failure { get; }
        public bool Won => Failure == RoadFailure.None;
        public long Score { get; }
        public long ShardMask { get; }
        public int Shards => RoadProgress.CountShards(ShardMask);
    }

    public static class RoadRules
    {
        public static bool CanExtend(RoadBoard board, int from, int cell, ulong visited) =>
            board.Contains(cell) && board[cell].Walkable &&
            (from < 0 ? cell == RoadBoard.Start : board.Adjacent(from, cell)) &&
            (visited & Bit(cell)) == 0;

        public static RoadRun Play(RoadBoard board, IReadOnlyList<int> path)
        {
            var steps = new List<RoadStep>();
            if (path == null || path.Count < 2)
                return new RoadRun(steps, RoadFailure.InvalidPath, 0, 0);

            ulong visited = 0;
            int previous = -1;
            for (int i = 0; i < path.Count; i++)
            {
                if (!CanExtend(board, previous, path[i], visited))
                    return new RoadRun(steps, RoadFailure.InvalidPath, 0, 0);

                previous = path[i];
                visited |= Bit(previous);
            }

            long score = 0;
            int combo = 0;
            long shards = 0;
            int finish = RoadBoard.Start;
            bool fromLauncher = false;
            ulong walked = 0;

            for (int i = 0; i < path.Count; i++)
            {
                int cell = path[i];
                AddTile(cell, fromLauncher ? RoadStepKind.Launch : RoadStepKind.Hop);
                walked |= Bit(cell);
                fromLauncher = false;
                finish = cell;
                if (board[cell].Kind != TileSpecialType.Launcher)
                    continue;

                int landing = board.LauncherLanding(cell);
                if (landing < 0)
                    return FailLaunch(RoadFailure.LauncherOffBoard, landing);
                if (!board[landing].Walkable)
                    return FailLaunch(RoadFailure.LauncherBlocked, landing);
                if ((walked & Bit(landing)) != 0)
                    return FailLaunch(RoadFailure.LauncherRevisit, landing);

                int drawnLanding = i + 1;
                while (drawnLanding < path.Count && path[drawnLanding] != landing)
                    drawnLanding++;

                if (drawnLanding == path.Count)
                {
                    if (i != path.Count - 1)
                        return FailLaunch(RoadFailure.LauncherMissedPath, landing);

                    while (true)
                    {
                        AddTile(landing, RoadStepKind.Launch);
                        walked |= Bit(landing);
                        finish = landing;
                        if (board[landing].Kind != TileSpecialType.Launcher)
                            break;

                        landing = board.LauncherLanding(landing);
                        if (landing < 0)
                            return FailLaunch(RoadFailure.LauncherOffBoard, landing);
                        if (!board[landing].Walkable)
                            return FailLaunch(RoadFailure.LauncherBlocked, landing);
                        if ((walked & Bit(landing)) != 0)
                            return FailLaunch(RoadFailure.LauncherRevisit, landing);
                    }

                    break;
                }

                if (drawnLanding > i + 1)
                {
                    for (int skipped = i + 1; skipped < drawnLanding; skipped++)
                    {
                        AddTile(path[skipped], RoadStepKind.Skipped);
                        walked |= Bit(path[skipped]);
                    }

                    score += GameBalance.LauncherShortcutBonus;
                    steps.Add(new RoadStep(RoadStepKind.ShortcutBonus, cell, score, combo));
                }

                i = drawnLanding - 1;
                fromLauncher = true;
            }

            RoadFailure result = board.CanFinish(finish)
                ? RoadFailure.None
                : board[finish].IsRegular ? RoadFailure.WrongColor : RoadFailure.ObstacleFinish;

            return new RoadRun(steps, result, score, shards);

            void AddTile(int cell, RoadStepKind kind)
            {
                RoadTile tile = board[cell];
                score = ScoreTile(board, cell, score);

                if (tile.Kind == TileSpecialType.Spike)
                    combo = 0;
                else if (tile.IsRegular)
                    combo = IsAddition(tile.Operation) ? combo + 1 : 0;

                if (tile.HasShard)
                    shards |= (long)Bit(cell);

                steps.Add(new RoadStep(kind, cell, score, combo, HasColorBonus(board, cell), tile.HasShard));
            }

            RoadRun FailLaunch(RoadFailure failure, int cell)
            {
                steps.Add(new RoadStep(RoadStepKind.Fall, cell, score, combo));
                return new RoadRun(steps, failure, score, shards);
            }
        }

        internal static ulong Bit(int cell) => 1UL << cell;

        internal static long ScoreTile(RoadBoard board, int cell, long score)
        {
            RoadTile tile = board[cell];
            if (tile.Kind == TileSpecialType.Spike)
                return score - GameBalance.SpikePenalty;

            if (!tile.IsRegular)
                return score;

            long updated = TileOperationCalculator.ApplyOperation(
                score, tile.Operation, TileOperationCalculator.DefaultValue(tile.Operation));
            return HasColorBonus(board, cell) ? updated * GameBalance.ColorBonusMultiplier : updated;
        }

        private static bool HasColorBonus(RoadBoard board, int cell) => board.CanFinish(cell);

        private static bool IsAddition(TileOperationType operation) =>
            operation == TileOperationType.PlusOne || operation == TileOperationType.PlusFive ||
            operation == TileOperationType.PlusTen;
    }
}
