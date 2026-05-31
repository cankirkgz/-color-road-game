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
                ApplyTile(path[i]);
            }

            Debug.Log($"Final Score Calculated | Score: {currentScore}");
        }

        private void ApplyTile(Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            int oldScore = currentScore;

            currentScore = TileOperationCalculator.ApplyOperation(
                currentScore,
                tile.OperationType,
                tile.OperationValue
            );

            Debug.Log($"Score Step {tile.TileId} | Tile: {tile.OperationType} {tile.OperationValue} | {oldScore} -> {currentScore}");
        }

        public void ResetScore()
        {
            currentScore = 0;
            Debug.Log("Score Reset | Current Score: 0");
        }
    }
}