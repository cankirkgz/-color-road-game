using System.Collections.Generic;
using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Score")]
        [SerializeField] private int currentScore;

        public int CurrentScore => currentScore;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ResetScore()
        {
            currentScore = 0;
            Debug.Log("Score Reset | Current Score: 0");
        }

        public void ApplyOperationFromTile(Tile tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("Score operation skipped. Tile is null.");
                return;
            }

            int oldScore = currentScore;

            currentScore = TileOperationCalculator.ApplyOperation(
                currentScore,
                tile.OperationType,
                tile.OperationValue
            );

            Debug.Log(
                $"Score Updated | Tile ID: {tile.TileId} | " +
                $"Operation: {tile.OperationType} {tile.OperationValue} | " +
                $"{oldScore} -> {currentScore}"
            );
        }

        public void CalculateScoreFromPath(List<Tile> path)
        {
            ResetScore();

            if (path == null || path.Count == 0)
            {
                Debug.Log("Score Calculation Skipped | Path is empty.");
                return;
            }

            for (int i = 0; i < path.Count; i++)
            {
                ApplyOperationFromTile(path[i]);
            }

            Debug.Log($"Final Score Calculated | Score: {currentScore}");
        }
    }
}