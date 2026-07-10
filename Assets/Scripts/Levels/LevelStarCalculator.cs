using System.Collections.Generic;
using System.Text;
using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Levels
{
    public static class LevelStarCalculator
    {
        private const int ColorBonusMultiplier = 2;
        private const int SpikeShortcutBonus = 50;

        public static int CalculateStars(LevelData levelData, int playerScore)
        {
            int maxScore = CalculateMaxPossibleScore(levelData);

            if (maxScore <= 0)
            {
                return 1;
            }

            float scoreRatio = (float)playerScore / maxScore;

            if (scoreRatio > 0.30f)
            {
                return 3;
            }

            if (scoreRatio >= 0.15f)
            {
                return 2;
            }

            return 1;
        }

        public static int CalculateMaxPossibleScore(LevelData levelData)
        {
            if (levelData == null || !levelData.HasValidSize())
            {
                Debug.LogWarning("Max score could not be calculated. LevelData is null or invalid.");
                return 0;
            }

            int startX = levelData.StartTileX;
            int startY = levelData.StartTileY;

            LevelTileData startTile = levelData.GetTileDataAt(startX, startY);

            if (startTile == null)
            {
                Debug.LogWarning($"Max score could not be calculated. Start tile missing at X:{startX}, Y:{startY}");
                return 0;
            }

            if (!CanUseTile(startTile))
            {
                Debug.LogError(
                    $"Max score could not be calculated. Start tile is blocked | " +
                    $"X:{startX}, Y:{startY}, " +
                    $"IsWalkable:{startTile.IsWalkable}, SpecialType:{startTile.SpecialType}"
                );

                return 0;
            }

            if (startTile.SpecialType == TileSpecialType.Spike)
            {
                Debug.LogError($"Max score could not be calculated. Start tile cannot be spike | X:{startX}, Y:{startY}");
                return 0;
            }

            bool[,] visited = new bool[levelData.GridWidth, levelData.GridHeight];

            int maxScore = int.MinValue;

            List<LevelTileData> currentPath = new List<LevelTileData>();
            List<LevelTileData> bestPath = new List<LevelTileData>();

            SearchBestPath(
                levelData,
                currentTile: startTile,
                startTile: startTile,
                currentScore: 0,
                visited: visited,
                currentPath: currentPath,
                bestPath: bestPath,
                maxScore: ref maxScore
            );

            if (bestPath.Count > 0)
            {
                Debug.Log(
                    $"Best Max Score Path | " +
                    $"Level: {levelData.LevelId} | " +
                    $"Max Score: {maxScore} | " +
                    $"Path: {BuildPathText(bestPath)}"
                );
            }

            if (maxScore == int.MinValue)
            {
                Debug.LogWarning("No successful path found for max score calculation.");
                return 0;
            }

            Debug.Log(
                $"Max Score Calculated | " +
                $"Level: {levelData.LevelId} | " +
                $"Max Score: {maxScore}"
            );

            return maxScore;
        }

        private static void SearchBestPath(
            LevelData levelData,
            LevelTileData currentTile,
            LevelTileData startTile,
            int currentScore,
            bool[,] visited,
            List<LevelTileData> currentPath,
            List<LevelTileData> bestPath,
            ref int maxScore
        )
        {
            visited[currentTile.X, currentTile.Y] = true;
            currentPath.Add(currentTile);

            int scoreAfterCurrentTile = ApplyTileScore(
                currentScore,
                currentTile,
                startTile
            );

            bool canFinishHere =
                currentTile.X != startTile.X ||
                currentTile.Y != startTile.Y;

            bool isSuccessfulFinish =
                canFinishHere &&
                CanUseTile(currentTile) &&
                currentTile.SpecialType != TileSpecialType.Spike &&
                currentTile.ColorType == startTile.ColorType;

            if (isSuccessfulFinish && scoreAfterCurrentTile > maxScore)
            {
                maxScore = scoreAfterCurrentTile;

                bestPath.Clear();
                bestPath.AddRange(currentPath);

                Debug.Log(
                    $"New Max Score Candidate | " +
                    $"Finish X:{currentTile.X}, Y:{currentTile.Y} | " +
                    $"Score: {maxScore} | " +
                    $"Path: {BuildPathText(bestPath)}"
                );
            }

            if (currentTile.SpecialType == TileSpecialType.Spike)
            {
                TryResolveSpikeBranch(
                    levelData,
                    currentTile,
                    startTile,
                    scoreAfterCurrentTile,
                    visited,
                    currentPath,
                    bestPath,
                    ref maxScore
                );

                currentPath.RemoveAt(currentPath.Count - 1);
                visited[currentTile.X, currentTile.Y] = false;
                return;
            }

            TryMoveToNeighbour(
                levelData,
                currentTile.X + 1,
                currentTile.Y,
                startTile,
                scoreAfterCurrentTile,
                visited,
                currentPath,
                bestPath,
                ref maxScore
            );

            TryMoveToNeighbour(
                levelData,
                currentTile.X - 1,
                currentTile.Y,
                startTile,
                scoreAfterCurrentTile,
                visited,
                currentPath,
                bestPath,
                ref maxScore
            );

            TryMoveToNeighbour(
                levelData,
                currentTile.X,
                currentTile.Y + 1,
                startTile,
                scoreAfterCurrentTile,
                visited,
                currentPath,
                bestPath,
                ref maxScore
            );

            TryMoveToNeighbour(
                levelData,
                currentTile.X,
                currentTile.Y - 1,
                startTile,
                scoreAfterCurrentTile,
                visited,
                currentPath,
                bestPath,
                ref maxScore
            );

            currentPath.RemoveAt(currentPath.Count - 1);
            visited[currentTile.X, currentTile.Y] = false;
        }

        private static void TryMoveToNeighbour(
            LevelData levelData,
            int targetX,
            int targetY,
            LevelTileData startTile,
            int currentScore,
            bool[,] visited,
            List<LevelTileData> currentPath,
            List<LevelTileData> bestPath,
            ref int maxScore
        )
        {
            if (!IsInsideGrid(levelData, targetX, targetY))
            {
                return;
            }

            if (visited[targetX, targetY])
            {
                return;
            }

            LevelTileData targetTile = levelData.GetTileDataAt(targetX, targetY);

            if (!CanUseTile(targetTile))
            {
                return;
            }

            SearchBestPath(
                levelData,
                targetTile,
                startTile,
                currentScore,
                visited,
                currentPath,
                bestPath,
                ref maxScore
            );
        }

        private static void TryResolveSpikeBranch(
            LevelData levelData,
            LevelTileData spikeTile,
            LevelTileData startTile,
            int currentScore,
            bool[,] visited,
            List<LevelTileData> currentPath,
            List<LevelTileData> bestPath,
            ref int maxScore
        )
        {
            LevelTileData pushTarget = GetSpikePushTarget(levelData, spikeTile);

            if (!CanUseTile(pushTarget) || visited[pushTarget.X, pushTarget.Y])
            {
                return;
            }

            // The drawn path may end on the spike. In that case the forced
            // landing tile is the real finish tile and still earns its score.
            int scoreAfterFinalLanding = ApplyTileScore(
                currentScore,
                pushTarget,
                startTile
            );

            bool finalLandingCanFinish =
                pushTarget.ColorType == startTile.ColorType &&
                pushTarget.SpecialType != TileSpecialType.Spike;

            if (finalLandingCanFinish && scoreAfterFinalLanding > maxScore)
            {
                maxScore = scoreAfterFinalLanding;

                bestPath.Clear();
                bestPath.AddRange(currentPath);
                bestPath.Add(pushTarget);

                Debug.Log(
                    $"New Max Score Candidate From Spike Final Landing | " +
                    $"Spike X:{spikeTile.X}, Y:{spikeTile.Y}, Direction:{spikeTile.SpikeDirection} | " +
                    $"Landing X:{pushTarget.X}, Y:{pushTarget.Y} | " +
                    $"Score:{maxScore} | " +
                    $"Path:{BuildPathText(bestPath)}"
                );
            }

            SearchSpikeSegment(
                levelData,
                spikeTile,
                pushTarget,
                startTile,
                currentScore,
                visited,
                currentPath,
                new List<LevelTileData>(),
                bestPath,
                ref maxScore
            );
        }

        private static void SearchSpikeSegment(
            LevelData levelData,
            LevelTileData currentTile,
            LevelTileData pushTarget,
            LevelTileData startTile,
            int currentScore,
            bool[,] visited,
            List<LevelTileData> currentPath,
            List<LevelTileData> segment,
            List<LevelTileData> bestPath,
            ref int maxScore
        )
        {
            List<LevelTileData> neighbours = GetUsableUnvisitedNeighbours(levelData, currentTile, visited);

            for (int i = 0; i < neighbours.Count; i++)
            {
                LevelTileData nextTile = neighbours[i];

                if (IsSameTile(nextTile, pushTarget))
                {
                    int scoreAfterSkippedTiles = currentScore;

                    for (int j = 0; j < segment.Count; j++)
                    {
                        scoreAfterSkippedTiles = ApplyTileScore(scoreAfterSkippedTiles, segment[j], startTile);
                    }

                    if (segment.Count > 0)
                    {
                        scoreAfterSkippedTiles += SpikeShortcutBonus;
                    }

                    SearchBestPath(
                        levelData,
                        pushTarget,
                        startTile,
                        scoreAfterSkippedTiles,
                        visited,
                        currentPath,
                        bestPath,
                        ref maxScore
                    );

                    continue;
                }

                visited[nextTile.X, nextTile.Y] = true;
                currentPath.Add(nextTile);
                segment.Add(nextTile);

                SearchSpikeSegment(
                    levelData,
                    nextTile,
                    pushTarget,
                    startTile,
                    currentScore,
                    visited,
                    currentPath,
                    segment,
                    bestPath,
                    ref maxScore
                );

                segment.RemoveAt(segment.Count - 1);
                currentPath.RemoveAt(currentPath.Count - 1);
                visited[nextTile.X, nextTile.Y] = false;
            }
        }

        private static List<LevelTileData> GetUsableUnvisitedNeighbours(
            LevelData levelData,
            LevelTileData tile,
            bool[,] visited
        )
        {
            List<LevelTileData> neighbours = new List<LevelTileData>();
            AddNeighbourIfUsable(levelData, tile.X + 1, tile.Y, visited, neighbours);
            AddNeighbourIfUsable(levelData, tile.X - 1, tile.Y, visited, neighbours);
            AddNeighbourIfUsable(levelData, tile.X, tile.Y + 1, visited, neighbours);
            AddNeighbourIfUsable(levelData, tile.X, tile.Y - 1, visited, neighbours);
            return neighbours;
        }

        private static void AddNeighbourIfUsable(
            LevelData levelData,
            int x,
            int y,
            bool[,] visited,
            List<LevelTileData> neighbours
        )
        {
            if (!IsInsideGrid(levelData, x, y) || visited[x, y])
            {
                return;
            }

            LevelTileData tile = levelData.GetTileDataAt(x, y);

            if (CanUseTile(tile))
            {
                neighbours.Add(tile);
            }
        }

        private static LevelTileData GetSpikePushTarget(LevelData levelData, LevelTileData spikeTile)
        {
            if (levelData == null || spikeTile == null)
            {
                return null;
            }

            int targetX = spikeTile.X;
            int targetY = spikeTile.Y;

            switch (spikeTile.SpikeDirection)
            {
                case GridDirection.Up:
                    targetY++;
                    break;
                case GridDirection.Down:
                    targetY--;
                    break;
                case GridDirection.Left:
                    targetX--;
                    break;
                case GridDirection.Right:
                    targetX++;
                    break;
                default:
                    return null;
            }

            return IsInsideGrid(levelData, targetX, targetY)
                ? levelData.GetTileDataAt(targetX, targetY)
                : null;
        }

        private static bool IsSameTile(LevelTileData firstTile, LevelTileData secondTile)
        {
            return firstTile != null &&
                   secondTile != null &&
                   firstTile.X == secondTile.X &&
                   firstTile.Y == secondTile.Y;
        }

        private static bool IsInsideGrid(LevelData levelData, int x, int y)
        {
            return x >= 0 &&
                   x < levelData.GridWidth &&
                   y >= 0 &&
                   y < levelData.GridHeight;
        }

        private static bool CanUseTile(LevelTileData tile)
        {
            if (tile == null)
            {
                return false;
            }

            if (!tile.IsWalkable)
            {
                return false;
            }

            if (tile.SpecialType == TileSpecialType.Wall)
            {
                return false;
            }

            return true;
        }

        private static int ApplyTileScore(
            int currentScore,
            LevelTileData tile,
            LevelTileData startTile
        )
        {
            if (!CanUseTile(tile))
            {
                Debug.LogWarning(
                    $"Score calculation skipped blocked tile | " +
                    $"X:{tile?.X}, Y:{tile?.Y}, SpecialType:{tile?.SpecialType}"
                );

                return currentScore;
            }

            int updatedScore = ApplyOperation(
                currentScore,
                tile.OperationType,
                tile.OperationValue
            );

            bool isStartTile =
                tile.X == startTile.X &&
                tile.Y == startTile.Y;

            bool shouldApplyColorBonus =
                !isStartTile &&
                tile.ColorType == startTile.ColorType;

            if (shouldApplyColorBonus)
            {
                updatedScore *= ColorBonusMultiplier;
            }

            return updatedScore;
        }

        private static int ApplyOperation(
            int currentScore,
            TileOperationType operationType,
            int operationValue
        )
        {
            switch (operationType)
            {
                case TileOperationType.PlusOne:
                case TileOperationType.PlusFive:
                case TileOperationType.PlusTen:
                case TileOperationType.MinusFive:
                case TileOperationType.MinusTen:
                case TileOperationType.MinusFifteen:
                    return currentScore + operationValue;

                case TileOperationType.MultiplyTwo:
                case TileOperationType.MultiplyThree:
                    return currentScore * operationValue;

                case TileOperationType.DivideTwo:
                    if (operationValue == 0)
                    {
                        return currentScore;
                    }

                    return currentScore / operationValue;

                default:
                    return currentScore;
            }
        }

        public static string GetStarsText(int starCount)
        {
            starCount = Mathf.Clamp(starCount, 1, 3);

            if (starCount == 1)
            {
                return "*";
            }

            if (starCount == 2)
            {
                return "* *";
            }

            return "* * *";
        }

        private static string BuildPathText(List<LevelTileData> path)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < path.Count; i++)
            {
                LevelTileData tile = path[i];

                builder.Append(
                    $"({tile.X},{tile.Y}) " +
                    $"{tile.OperationType} {tile.OperationValue} " +
                    $"{tile.ColorType} {tile.SpecialType}"
                );

                if (tile.SpecialType == TileSpecialType.Spike)
                {
                    builder.Append($" {tile.SpikeDirection}");
                }

                if (i < path.Count - 1)
                {
                    builder.Append(" -> ");
                }
            }

            return builder.ToString();
        }
    }
}
