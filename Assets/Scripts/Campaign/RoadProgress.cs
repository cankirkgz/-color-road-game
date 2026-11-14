using System;
using System.Collections.Generic;

namespace RenkYolu.Campaign
{
    [Serializable]
    public sealed class RoadLevelRecord
    {
        public int level;
        public long best;
        public int stars;
        public long shardMask;
    }

    [Serializable]
    public sealed class RoadProgress
    {
        public const int ShardsPerCube = 5;
        public const int CubeCount = 4;

        public int seed;
        public int unlocked = 1;
        public int cube;
        public bool muted;
        public long legacyScore;
        public List<RoadLevelRecord> levels = new List<RoadLevelRecord>();

        public long TotalScore
        {
            get
            {
                long total = legacyScore;
                foreach (RoadLevelRecord record in levels)
                    total += record.best;
                return total;
            }
        }

        public int TotalShards
        {
            get
            {
                int total = 0;
                foreach (RoadLevelRecord record in levels)
                    total += CountShards(record.shardMask);
                return total;
            }
        }

        public int AvailableCubes => Math.Min(CubeCount, 1 + TotalShards / ShardsPerCube);

        public long TotalStars
        {
            get
            {
                long total = 0;
                foreach (RoadLevelRecord record in levels)
                    total += record.stars;
                return total;
            }
        }

        public RoadProgress(int worldSeed)
        {
            seed = worldSeed;
        }

        public RoadLevelRecord Find(int level) => levels.Find(record => record.level == level);

        public bool RefreshEarlyStars()
        {
            bool changed = false;
            foreach (RoadLevelRecord record in levels)
            {
                if (record.level < 1 || record.level > RoadDifficulty.LastRebalancedLevel)
                    continue;

                int stars = RoadGenerator.Generate(seed, record.level).Stars(record.best);
                if (stars != record.stars)
                {
                    record.stars = stars;
                    changed = true;
                }
            }

            return changed;
        }

        public void RecordWin(int level, long score, int stars, long shardMask)
        {
            if (level < 1 || level > unlocked)
                return;

            RoadLevelRecord record = Find(level);
            if (record == null)
            {
                record = new RoadLevelRecord { level = level };
                levels.Add(record);
            }

            record.best = Math.Max(record.best, score);
            record.stars = Math.Max(record.stars, stars);
            record.shardMask |= shardMask;

            if (level == unlocked && unlocked < int.MaxValue)
                unlocked++;
        }

        internal static int CountShards(long mask)
        {
            var remaining = (ulong)mask;
            int count = 0;
            while (remaining != 0)
            {
                remaining &= remaining - 1;
                count++;
            }

            return count;
        }
    }
}
