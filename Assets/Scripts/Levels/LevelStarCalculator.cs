using System.Collections.Generic;
using System.Text;
using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Levels
{
    public static class LevelStarCalculator
    {
        private const int ColorBonusMultiplier = 2;

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

            // Oyunun şu anki kuralı:
            // Kullanıcı sol alt köşeden başlar.
            int startX = 0;
            int startY = 0;

            LevelTileData startTile = levelData.GetTileDataAt(startX, startY);

            if (startTile == null)
            {
                Debug.LogWarning($"Max score could not be calculated. Start tile missing at X:{startX}, Y:{startY}");
                return 0;
            }

            if (!startTile.IsWalkable)
            {
                Debug.LogWarning($"Max score could not be calculated. Start tile is not walkable at X:{startX}, Y:{startY}");
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

            if (targetTile == null)
            {
                return;
            }

            if (!targetTile.IsWalkable)
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

        private static bool IsInsideGrid(LevelData levelData, int x, int y)
        {
            return x >= 0 &&
                   x < levelData.GridWidth &&
                   y >= 0 &&
                   y < levelData.GridHeight;
        }

        private static int ApplyTileScore(
            int currentScore,
            LevelTileData tile,
            LevelTileData startTile
        )
        {
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

                builder.Append($"({tile.X},{tile.Y}) {tile.OperationType} {tile.OperationValue} {tile.ColorType}");

                if (i < path.Count - 1)
                {
                    builder.Append(" -> ");
                }
            }

            return builder.ToString();
        }
    }
}