using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using RenkYolu.Campaign;
using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Tests
{
    public sealed class RoadCampaignTests
    {
        private static readonly RoadTile Start = new RoadTile(TileColorType.Red, TileOperationType.PlusFive);
        private static readonly RoadTile Plain = new RoadTile(TileColorType.Blue, TileOperationType.PlusTen);
        private static readonly RoadTile Wall = new RoadTile(TileColorType.Blue, TileOperationType.PlusOne, TileSpecialType.Wall);
        private static readonly RoadTile Spike = new RoadTile(TileColorType.Blue, TileOperationType.PlusTen, TileSpecialType.Spike);

        [Test]
        public void OperationsAndColorApplyInOrder()
        {
            RoadBoard board = Board((1, Finish()), (4, new RoadTile(TileColorType.Green, TileOperationType.DivideTwo)),
                (5, Finish(TileOperationType.MultiplyThree)));
            RoadRun result = RoadRules.Play(board, new[] { 0, 1, 4, 5 });
            Assert.That(result.Won, Is.True);
            Assert.That(result.Steps.Select(s => s.Score), Is.EqualTo(new long[] { 5, 30, 15, 90 }));
            Assert.That(result.Steps.Select(s => s.Combo), Is.EqualTo(new[] { 1, 2, 0, 0 }));
            Assert.That(result.Steps.Select(s => s.ColorBonus), Is.EqualTo(new[] { false, true, false, true }));
        }

        [Test]
        public void SpikeAppliesPenaltyClearsComboAndCannotFinish()
        {
            RoadBoard board = Board((2, Spike), (5, Finish()));
            RoadRun result = RoadRules.Play(board, new[] { 0, 1, 2, 5 });
            Assert.That(result.Won, Is.True);
            Assert.That(result.Score, Is.EqualTo(-50));
            Assert.That(result.Steps[2].Combo, Is.Zero);
            Assert.That(result.Steps[2].Score, Is.EqualTo(-35));
            Assert.That(RoadRules.Play(board, new[] { 0, 1, 2 }).Failure, Is.EqualTo(RoadFailure.ObstacleFinish));
        }

        [TestCase(new[] { 0 })]
        [TestCase(new[] { 1, 2 })]
        [TestCase(new[] { 0, 4 })]
        [TestCase(new[] { 0, 1, 0 })]
        [TestCase(new[] { 0, 3 })]
        [TestCase(new[] { 0, 1, 9 })]
        public void RejectsIllegalPaths(int[] route)
        {
            Assert.That(RoadRules.Play(Board((3, Wall)), route).Failure, Is.EqualTo(RoadFailure.InvalidPath));
        }

        [Test]
        public void MatchingColorIsRequired()
        {
            Assert.That(RoadRules.Play(Board(), new[] { 0, 1 }).Failure, Is.EqualTo(RoadFailure.WrongColor));
            Assert.That(RoadSolver.Solve(Board()).IsSolvable, Is.False);
        }

        [Test]
        public void LastDrawnLauncherCanFinishOnItsLanding()
        {
            RoadBoard board = Board((1, Launcher(GridDirection.Up)), (4, Finish()));
            RoadRun result = RoadRules.Play(board, new[] { 0, 1 });
            Assert.That(result.Won, Is.True);
            Assert.That(result.Score, Is.EqualTo(30));
            Assert.That(result.Steps.Last().Cell, Is.EqualTo(4));
            Assert.That(result.Steps.Last().Kind, Is.EqualTo(RoadStepKind.Launch));
            Assert.That(RoadRules.Play(board, new[] { 0, 1, 4 }).Score, Is.EqualTo(30));
        }

        [Test]
        public void ShortcutProcessesSkippedOperationsBeforeBonusAndLanding()
        {
            RoadBoard board = Board((1, Launcher(GridDirection.Up)),
                (2, new RoadTile(TileColorType.Blue, TileOperationType.PlusTen, hasShard: true)),
                (4, Finish(TileOperationType.MultiplyTwo, true)), (5, Spike));
            RoadRun result = RoadRules.Play(board, new[] { 0, 1, 2, 5, 4 });
            Assert.That(result.Won, Is.True);
            Assert.That(result.Score, Is.EqualTo(60));
            Assert.That(result.Shards, Is.EqualTo(2));
            Assert.That(result.Steps.Select(step => step.Kind), Is.EqualTo(new[]
            {
                RoadStepKind.Hop, RoadStepKind.Hop, RoadStepKind.Skipped,
                RoadStepKind.Skipped, RoadStepKind.ShortcutBonus, RoadStepKind.Launch
            }));
            Assert.That(result.Steps.Select(step => step.Score), Is.EqualTo(new long[] { 5, 5, 15, -35, 15, 60 }));
        }

        [TestCase(GridDirection.Down, false, false, RoadFailure.LauncherOffBoard)]
        [TestCase(GridDirection.Up, true, false, RoadFailure.LauncherBlocked)]
        [TestCase(GridDirection.Left, false, false, RoadFailure.LauncherRevisit)]
        [TestCase(GridDirection.Up, false, true, RoadFailure.LauncherMissedPath)]
        public void InvalidLauncherDestinationsFail(GridDirection direction, bool blocked, bool keepDrawing, RoadFailure reason)
        {
            RoadBoard board = Board((1, Launcher(direction)), (4, blocked ? Wall : Finish()));
            RoadRun result = RoadRules.Play(board, keepDrawing ? new[] { 0, 1, 2 } : new[] { 0, 1 });
            Assert.That(result.Failure, Is.EqualTo(reason));
            Assert.That(result.Steps.Last().Kind, Is.EqualTo(RoadStepKind.Fall));
        }

        [Test]
        public void ChainResolvesPhysicsAndRejectsLoops()
        {
            RoadBoard board = Board((1, Launcher(GridDirection.Up)), (4, Launcher(GridDirection.Up)), (7, Finish()));
            Assert.That(RoadRules.Play(board, new[] { 0, 1 }).Steps.Select(s => s.Cell), Is.EqualTo(new[] { 0, 1, 4, 7 }));
            Assert.That(RoadRules.Play(board, new[] { 0, 1, 4, 7 }).Won, Is.True);
            RoadBoard loop = Board((1, Launcher(GridDirection.Up)), (4, Launcher(GridDirection.Down)), (7, Finish()));
            Assert.That(RoadRules.Play(loop, new[] { 0, 1 }).Failure, Is.EqualTo(RoadFailure.LauncherRevisit));
            AssertExact(board);
            AssertExact(loop);
        }

        [Test]
        public void SolverMatchesIndependentEnumerationOnVariedBoards()
        {
            AssertExact(Board((1, Launcher(GridDirection.Up)), (2, Spike), (4, Finish()), (6, Finish())));
            AssertExact(Board((1, Launcher(GridDirection.Up)), (4, Finish(TileOperationType.DivideTwo)),
                (5, Spike), (6, Finish()), (7, Launcher(GridDirection.Left))));

            foreach (int seed in new[] { 521, 92, -2115, 0 })
            {
                foreach (int level in new[] { 1, 6, 8, 12, 16, 17, 32, 49, 85, 160 })
                    AssertExact(RoadGenerator.Generate(seed, level).Board);
            }
        }

        [Test]
        public void FirstLevelStarsUseTheTrueMaximumEvenWhenTwoSquaresCanWin()
        {
            RoadLevel level = RoadGenerator.Generate(86421116, 1);
            RoadRun shortRun = RoadRules.Play(level.Board, level.Solution.ShortRoute);
            Assert.That(level.Solution.Maximum, Is.EqualTo(1728));
            Assert.That(shortRun.Score, Is.EqualTo(20));
            Assert.That(level.Stars(shortRun.Score), Is.EqualTo(1));
            Assert.That(level.TwoStarScore, Is.EqualTo(208));
            Assert.That(level.ThreeStarScore, Is.EqualTo(432));
            Assert.That(level.Stars(431), Is.EqualTo(2));
            Assert.That(level.Stars(432), Is.EqualTo(3));
            Assert.That(level.Solution.BestWithin(10), Is.GreaterThanOrEqualTo(432));
        }

        [Test]
        public void RebalancedStarsUpdateExistingRecordsWithoutLosingProgress()
        {
            var progress = new RoadProgress(86421116);
            progress.RecordWin(1, 20, 3, 2);
            Assert.That(progress.RefreshEarlyStars(), Is.True);
            Assert.That(progress.TotalStars, Is.EqualTo(1));
            Assert.That(progress.TotalScore, Is.EqualTo(20));
            Assert.That(progress.TotalShards, Is.EqualTo(1));
            Assert.That(progress.unlocked, Is.EqualTo(2));
            Assert.That(progress.RefreshEarlyStars(), Is.False);
        }

        [Test]
        public void CampaignSupportsFirstTwoHundredAndLaterLevels()
        {
            var timer = Stopwatch.StartNew();
            int count = 0;
            long maxScore = 0;
            foreach (int seed in new[] { 0, 201, -1234567, int.MaxValue })
            {
                foreach (int number in Enumerable.Range(1, 200).Concat(new[] { 512, 10000, int.MaxValue }))
                {
                    RoadLevel level = RoadGenerator.Generate(seed, number);
                    RoadRun best = RoadRules.Play(level.Board, level.Solution.BestRoute);
                    RoadRun shortRun = RoadRules.Play(level.Board, level.Solution.ShortRoute);
                    string label = "Seed " + seed + ", level " + number;
                    Assert.That(best.Won, Is.True, label);
                    Assert.That(best.Score, Is.EqualTo(level.Solution.Maximum).And.Positive, label);
                    Assert.That(shortRun.Won, Is.True, label);
                    Assert.That(shortRun.Score, Is.Positive, label);
                    Assert.That(level.Stars(0), Is.EqualTo(1), label);
                    Assert.That(level.Stars(level.TwoStarScore), Is.EqualTo(2), label);
                    Assert.That(level.Stars(level.ThreeStarScore), Is.EqualTo(3), label);
                    Assert.That(level.ThreeStarScore, Is.LessThanOrEqualTo(best.Score), label);
                    Assert.That(Enumerable.Range(0, level.Board.Count).Count(cell => level.Board[cell].HasShard),
                        Is.InRange(1, 2), label);
                    if (number == 1)
                    {
                        Assert.That(level.Stars(shortRun.Score), Is.EqualTo(1), label);
                        Assert.That(shortRun.Shards, Is.EqualTo(1), label);
                    }
                    Assert.That(level.ThreeStarScore * 100,
                        Is.GreaterThanOrEqualTo(level.Solution.Maximum * 25), label);
                    count++;
                    maxScore = Math.Max(maxScore, best.Score);
                }
            }

            TestContext.WriteLine($"{count} boards in {timer.ElapsedMilliseconds} ms; highest score {maxScore}.");
        }

        [Test]
        public void SameLevelRetainsMapAndLargerBoardsGetMoreTimeAndHigherStarTargets()
        {
            RoadLevel a = RoadGenerator.Generate(2715, 100);
            RoadLevel b = RoadGenerator.Generate(2715, 100);
            RoadLevel c = RoadGenerator.Generate(4298, 100);
            IEnumerable<int> cells = Enumerable.Range(0, a.Board.Count);
            Assert.That(cells.Select(i => a.Board[i]), Is.EqualTo(cells.Select(i => b.Board[i])));
            Assert.That(cells.Select(i => a.Board[i]), Is.Not.EqualTo(cells.Select(i => c.Board[i])));
            Assert.That(a.Solution.Maximum, Is.EqualTo(b.Solution.Maximum));

            RoadDifficulty opening = RoadDifficulty.ForLevel(1);
            RoadDifficulty medium = RoadDifficulty.ForLevel(17);
            RoadDifficulty hard = RoadDifficulty.ForLevel(49);
            RoadDifficulty endless = RoadDifficulty.ForLevel(85);
            Assert.That(new[] { opening.Size, medium.Size, hard.Size, endless.Size }, Is.EqualTo(new[] { 4, 5, 6, 7 }));
            Assert.That(endless.DrawingSeconds, Is.GreaterThan(opening.DrawingSeconds));
            Assert.That(endless.MemorySeconds, Is.GreaterThan(opening.MemorySeconds));
            Assert.That(endless.ThreeStarPercent, Is.GreaterThan(opening.ThreeStarPercent));
        }

        [Test]
        public void ProgressUnlocksOnceAndStoresBestScoresAndDistinctShards()
        {
            var progress = new RoadProgress(546);
            progress.RecordWin(2, 10000, 3, 1024);
            Assert.That(progress.unlocked, Is.EqualTo(1));
            progress.RecordWin(1, 50, 2, 2);
            progress.RecordWin(1, 40, 1, 2);
            progress.RecordWin(1, 45, 3, 4);
            progress.RecordWin(2, 85, 2, 6);
            progress.RecordWin(3, 10, 1, 8);
            Assert.That(progress.unlocked, Is.EqualTo(4));
            Assert.That(progress.TotalScore, Is.EqualTo(145));
            Assert.That(progress.TotalStars, Is.EqualTo(6));
            Assert.That(progress.TotalShards, Is.EqualTo(5));
            Assert.That(progress.AvailableCubes, Is.EqualTo(2));

            progress.cube = 1;
            progress.muted = true;
            var saved = JsonUtility.FromJson<RoadProgress>(JsonUtility.ToJson(progress));
            Assert.That(saved.seed, Is.EqualTo(546));
            Assert.That(saved.unlocked, Is.EqualTo(4));
            Assert.That(saved.Find(1).best, Is.EqualTo(50));
            Assert.That(saved.Find(1).shardMask, Is.EqualTo(6));
            Assert.That(saved.cube, Is.EqualTo(1));
            Assert.That(saved.muted, Is.True);
        }

        private static void AssertExact(RoadBoard board)
        {
            long expected = long.MinValue;
            var route = new List<int> { 0 };
            TryEveryPath(0);
            RoadSolution solution = RoadSolver.Solve(board);
            Assert.That(solution.IsSolvable, Is.EqualTo(expected != long.MinValue));
            Assert.That(solution.Maximum, Is.EqualTo(expected == long.MinValue ? 0 : expected));
            if (solution.IsSolvable)
            {
                RoadRun replay = RoadRules.Play(board, solution.BestRoute);
                Assert.That(replay.Won, Is.True);
                Assert.That(replay.Score, Is.EqualTo(expected));
            }

            void TryEveryPath(int cell)
            {
                if (route.Count > 1)
                {
                    RoadRun result = RoadRules.Play(board, route);
                    if (result.Won)
                        expected = Math.Max(expected, result.Score);
                }

                foreach (int next in board.Neighbours(cell))
                {
                    if (route.Contains(next))
                        continue;

                    route.Add(next);
                    TryEveryPath(next);
                    route.RemoveAt(route.Count - 1);
                }
            }
        }

        private static RoadBoard Board(params (int cell, RoadTile tile)[] edits)
        {
            var tiles = Enumerable.Repeat(Plain, 9).ToArray();
            tiles[0] = Start;
            foreach (var edit in edits)
                tiles[edit.cell] = edit.tile;
            return new RoadBoard(3, tiles);
        }

        private static RoadTile Launcher(GridDirection direction) =>
            new RoadTile(TileColorType.Blue, TileOperationType.PlusOne, TileSpecialType.Launcher, direction);

        private static RoadTile Finish(TileOperationType operation = TileOperationType.PlusTen, bool shard = false) =>
            new RoadTile(TileColorType.Red, operation, hasShard: shard);
    }
}
