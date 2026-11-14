using System;
using System.Collections.Generic;
using RenkYolu.Grid;

namespace RenkYolu.Campaign
{
    public readonly struct RoadDifficulty
    {
        private RoadDifficulty(int level, int size, int colors, int memory, int drawing, int twoStars, int threeStars)
        {
            Level = level;
            Size = size;
            Colors = colors;
            MemorySeconds = memory;
            DrawingSeconds = drawing;
            TwoStarPercent = twoStars;
            ThreeStarPercent = threeStars;
        }

        public int Level { get; }
        public int Size { get; }
        public int Colors { get; }
        public int MemorySeconds { get; }
        public int DrawingSeconds { get; }
        public int TwoStarPercent { get; }
        public int ThreeStarPercent { get; }

        public static RoadDifficulty ForLevel(int level)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException(nameof(level));

            int size = level < 17 ? 4 : level < 49 ? 5 : level < 85 ? 6 : 7;
            int colors = level < 5 ? 3 : level < 25 ? 4 : level < 66 ? 5 : 6;
            int memory = level == 1 ? 10 : size * 2 + (level < 6 ? 1 : 0);
            int drawing = 20 + (size - 4) * 7 + (level < 4 ? 5 : 0);
            int twoStars = 12 + Math.Min(level - 1, 140) / 6;
            int threeStars = 24 + Math.Min(level - 1, 160) / 4;
            return new RoadDifficulty(level, size, colors, memory, drawing, twoStars, threeStars);
        }
    }

    public sealed class RoadLevel
    {
        internal RoadLevel(RoadBoard board, RoadDifficulty difficulty, RoadSolution solution)
        {
            Board = board;
            Difficulty = difficulty;
            Solution = solution;
            long twoStars = PercentOfMaximum(difficulty.TwoStarPercent);
            long threeStars = PercentOfMaximum(difficulty.ThreeStarPercent);
            if (difficulty.Level <= 8)
            {
                int forgivingLength = difficulty.Level == 1 ? 2 : 6;
                long easyScore = solution.BestWithin(forgivingLength);
                threeStars = Math.Min(threeStars, easyScore);
            }

            ThreeStarScore = Math.Max(2, threeStars);
            TwoStarScore = Math.Min(twoStars, ThreeStarScore - 1);
        }

        public RoadBoard Board { get; }
        public RoadDifficulty Difficulty { get; }
        public RoadSolution Solution { get; }
        public long TwoStarScore { get; }
        public long ThreeStarScore { get; }

        public int Stars(long score) => score >= ThreeStarScore ? 3 : score >= TwoStarScore ? 2 : 1;

        private long PercentOfMaximum(int percent) =>
            Math.Max(1, (Solution.Maximum * percent + 99) / 100);
    }

    public static class RoadGenerator
    {
        private static readonly TileOperationType[] Additions =
        {
            TileOperationType.PlusOne, TileOperationType.PlusFive, TileOperationType.PlusTen
        };

        private static readonly TileOperationType[] Subtractions =
        {
            TileOperationType.MinusFive, TileOperationType.MinusTen, TileOperationType.MinusFifteen
        };

        public static RoadLevel Generate(int worldSeed, int level)
        {
            RoadDifficulty difficulty = RoadDifficulty.ForLevel(level);
            var random = new RoadRandom(worldSeed, level);
            int size = difficulty.Size;
            bool[] open = MakeLayout(random, difficulty);
            int[] distance = Distances(size, open, out int[] parent);
            var order = new List<int>();
            for (int cell = 1; cell < open.Length; cell++)
            {
                if (distance[cell] > 0)
                    order.Add(cell);
            }

            Shuffle(order, random);
            int minimumDistance = level < 5 ? 3 : level < 49 ? size : size + 1;
            int maximumDistance = level <= 5 ? 4 : level <= 8 ? 5 : int.MaxValue;
            int finish = order.Find(cell => distance[cell] >= minimumDistance && distance[cell] <= maximumDistance);
            if (finish == 0)
                finish = order[0];

            var guide = new HashSet<int>();
            for (int cell = finish; cell >= 0; cell = parent[cell])
                guide.Add(cell);

            var palette = new List<TileColorType>
            {
                TileColorType.Red, TileColorType.Blue, TileColorType.Green,
                TileColorType.Yellow, TileColorType.Purple, TileColorType.Orange
            };
            Shuffle(palette, random);
            var cells = new RoadTile[open.Length];
            cells[0] = new RoadTile(palette[0], TileOperationType.PlusFive);

            for (int cell = 1; cell < cells.Length; cell++)
            {
                if (!open[cell])
                {
                    cells[cell] = new RoadTile(palette[1], TileOperationType.PlusOne, TileSpecialType.Wall);
                    continue;
                }

                TileColorType color = cell == finish ? palette[0] : palette[random.Next(1, difficulty.Colors)];
                TileOperationType operation = guide.Contains(cell)
                    ? (cell == finish ? TileOperationType.PlusTen : Additions[random.Next(1, Additions.Length)])
                    : RandomOperation(random, level);
                cells[cell] = new RoadTile(color, operation);
            }

            if (level == 1)
            {
                int neighbour = finish;
                while (parent[neighbour] > RoadBoard.Start)
                    neighbour = parent[neighbour];

                cells[neighbour] = new RoadTile(palette[0], TileOperationType.PlusFive);
            }

            int extraFinishes = 1 + (size - 4) / 2;
            foreach (int cell in order)
            {
                if (extraFinishes == 0)
                    break;
                if (guide.Contains(cell) || distance[cell] < minimumDistance ||
                    (level <= 5 && distance[cell] > 5))
                    continue;

                cells[cell] = new RoadTile(palette[0], cells[cell].Operation);
                extraFinishes--;
            }

            PlaceObstacles(cells, order, guide, size, level, random);

            var board = new RoadBoard(size, cells);
            RoadSolution solution = RoadSolver.Solve(board);
            if (!solution.IsSolvable || solution.Maximum <= 0)
                throw new InvalidOperationException("The generated board lost its guaranteed scoring route.");

            int shards = level >= 17 ? 2 : 1;
            if (level <= 3)
            {
                MarkShard(finish);
                shards--;
            }

            if (level == 1)
            {
                MarkShard(solution.ShortRoute[1]);
            }

            foreach (int cell in order)
            {
                if (shards == 0)
                    break;

                if (cells[cell].IsRegular && !cells[cell].HasShard &&
                    (solution.WinningTiles & RoadRules.Bit(cell)) != 0)
                {
                    MarkShard(cell);
                    shards--;
                }
            }

            return new RoadLevel(new RoadBoard(size, cells), difficulty, solution);

            void MarkShard(int cell)
            {
                RoadTile tile = cells[cell];
                cells[cell] = new RoadTile(tile.Color, tile.Operation, hasShard: true);
            }
        }

        private static bool[] MakeLayout(RoadRandom random, RoadDifficulty difficulty)
        {
            int size = difficulty.Size;
            var open = new bool[size * size];
            for (int cell = 0; cell < open.Length; cell++)
                open[cell] = true;

            if (size >= 6)
            {
                // Four small areas with at most three connecting doors bound the exact route search.
                const int divider = 3;
                for (int index = 0; index < size; index++)
                {
                    open[index * size + divider] = false;
                    open[divider * size + index] = false;
                }

                var gates = new List<int>
                {
                    random.Next(0, divider) * size + divider,
                    random.Next(divider + 1, size) * size + divider,
                    divider * size + random.Next(0, divider),
                    divider * size + random.Next(divider + 1, size)
                };
                gates.RemoveAt(random.Next(gates.Count));
                foreach (int gate in gates)
                    open[gate] = true;
            }

            int walls = size >= 6 ? (difficulty.Level >= 105 ? 2 : 1) :
                difficulty.Level < 6 ? 0 : size == 4 ? 1 : 3;
            var order = new List<int>();
            for (int cell = 1; cell < open.Length; cell++)
            {
                if (open[cell])
                    order.Add(cell);
            }

            Shuffle(order, random);
            foreach (int wall in order)
            {
                if (walls == 0)
                    break;
                if (wall == 1 || wall == size)
                    continue;

                open[wall] = false;
                int[] distance = Distances(size, open, out _);
                if (AllOpenCellsReachable(open, distance))
                    walls--;
                else
                    open[wall] = true;
            }

            return open;
        }

        private static bool AllOpenCellsReachable(bool[] open, int[] distance)
        {
            for (int cell = 1; cell < open.Length; cell++)
            {
                if (open[cell] && distance[cell] < 0)
                    return false;
            }

            return true;
        }

        private static void PlaceObstacles(
            RoadTile[] cells, List<int> order, HashSet<int> guide, int size, int level, RoadRandom random)
        {
            int launchers = level < 8 ? 0 : level < 49 ? 1 : level < 110 ? 2 : 3;
            int spikes = level < 12 ? 0 : level < 38 ? 1 : level < 80 ? 2 : 3;
            var reservedLandings = new HashSet<int>();

            foreach (int cell in order)
            {
                if (launchers == 0)
                    break;
                if (guide.Contains(cell) || cells[cell].Color == cells[0].Color || reservedLandings.Contains(cell))
                    continue;

                var landings = new List<int>(4);
                foreach (int neighbour in AdjacentCells(size, cell))
                {
                    if (neighbour != RoadBoard.Start && cells[neighbour].IsRegular)
                        landings.Add(neighbour);
                }

                if (landings.Count == 0)
                    continue;

                int landing = landings[random.Next(landings.Count)];
                int delta = landing - cell;
                GridDirection direction = delta == 1 ? GridDirection.Right : delta == -1 ? GridDirection.Left :
                    delta > 0 ? GridDirection.Up : GridDirection.Down;
                cells[cell] = new RoadTile(cells[cell].Color, cells[cell].Operation, TileSpecialType.Launcher, direction);
                reservedLandings.Add(landing);
                launchers--;
            }

            foreach (int cell in order)
            {
                if (spikes == 0)
                    break;
                if (guide.Contains(cell) || !cells[cell].IsRegular || cells[cell].Color == cells[0].Color ||
                    reservedLandings.Contains(cell))
                    continue;

                cells[cell] = new RoadTile(cells[cell].Color, cells[cell].Operation, TileSpecialType.Spike);
                spikes--;
            }
        }

        private static TileOperationType RandomOperation(RoadRandom random, int level)
        {
            int roll = random.Next(100);
            int addition = level < 4 ? 84 : level < 17 ? 70 : 62;
            if (roll < addition)
                return Additions[random.Next(Additions.Length)];
            if (roll < 91)
                return Subtractions[random.Next(level < 17 ? 2 : Subtractions.Length)];
            if (roll < 95 || level == 1)
                return TileOperationType.MultiplyTwo;
            if (roll < 98)
                return TileOperationType.DivideTwo;

            return TileOperationType.MultiplyThree;
        }

        private static int[] Distances(int size, bool[] open, out int[] parent)
        {
            var distance = new int[open.Length];
            parent = new int[open.Length];
            Array.Fill(distance, -1);
            Array.Fill(parent, -1);
            var queue = new Queue<int>();
            distance[0] = 0;
            queue.Enqueue(RoadBoard.Start);
            while (queue.Count > 0)
            {
                int cell = queue.Dequeue();
                foreach (int next in AdjacentCells(size, cell))
                {
                    if (!open[next] || distance[next] >= 0)
                        continue;

                    parent[next] = cell;
                    distance[next] = distance[cell] + 1;
                    queue.Enqueue(next);
                }
            }

            return distance;
        }

        private static IEnumerable<int> AdjacentCells(int size, int cell)
        {
            if (cell % size > 0)
                yield return cell - 1;
            if (cell % size + 1 < size)
                yield return cell + 1;
            if (cell >= size)
                yield return cell - size;
            if (cell < size * (size - 1))
                yield return cell + size;
        }

        private static void Shuffle<T>(IList<T> list, RoadRandom random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int other = random.Next(i + 1);
                (list[i], list[other]) = (list[other], list[i]);
            }
        }
    }
}
