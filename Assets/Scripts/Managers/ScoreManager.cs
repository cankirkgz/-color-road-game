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

        [Header("Color Bonus")]
        [SerializeField] private int colorBonusMultiplier = 2;
        [SerializeField] private TileColorType startColor;
        [SerializeField] private int startTileId = -1;
        [SerializeField] private bool hasStartColor;

        [Header("Combo")]
        [SerializeField] private int comboCounter;
        [SerializeField] private int minimumComboToShow = 2;

        public int CurrentScore => currentScore;
        public TileColorType StartColor => startColor;
        public bool HasStartColor => hasStartColor;
        public int ComboCounter => comboCounter;
        public int MinimumComboToShow => minimumComboToShow;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void SetStartTile(Tile startTile)
        {
            if (startTile == null)
            {
                Debug.LogWarning("Start tile could not be set. Tile is null.");
                return;
            }

            startColor = startTile.ColorType;
            startTileId = startTile.TileId;
            hasStartColor = true;

            Debug.Log(
                $"Start Color Set | " +
                $"Start Tile ID: {startTileId} | " +
                $"Start Color: {startColor}"
            );
        }

        public void ResetScore()
        {
            currentScore = 0;
            comboCounter = 0;

            Debug.Log("Score Reset | Current Score: 0 | Combo Reset");
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
                $"Score Updated | " +
                $"Tile ID: {tile.TileId} | " +
                $"Color: {tile.ColorType} | " +
                $"Operation: {tile.OperationType} | " +
                $"Value: {tile.OperationValue} | " +
                $"Score: {oldScore} -> {currentScore}"
            );
        }

        public int UpdateComboFromTile(Tile tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("Combo update skipped. Tile is null.");
                return 0;
            }

            if (IsPositiveOperation(tile.OperationType))
            {
                comboCounter++;

                Debug.Log(
                    $"Combo Increased | " +
                    $"Tile ID: {tile.TileId} | " +
                    $"Operation: {tile.OperationType} | " +
                    $"Combo: x{comboCounter}"
                );

                return comboCounter;
            }

            if (comboCounter > 0)
            {
                Debug.Log(
                    $"Combo Reset | " +
                    $"Tile ID: {tile.TileId} | " +
                    $"Operation: {tile.OperationType}"
                );
            }

            comboCounter = 0;
            return 0;
        }

        public bool ShouldShowCombo()
        {
            return comboCounter >= minimumComboToShow;
        }

        public bool TryApplyColorBonus(Tile tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("Color bonus skipped. Tile is null.");
                return false;
            }

            if (!hasStartColor)
            {
                Debug.LogWarning("Color bonus skipped. Start color is not set.");
                return false;
            }

            if (tile.TileId == startTileId)
            {
                Debug.Log("Color bonus skipped. This is the start tile.");
                return false;
            }

            if (tile.ColorType != startColor)
            {
                return false;
            }

            int oldScore = currentScore;
            currentScore *= colorBonusMultiplier;

            Debug.Log(
                $"Color Bonus Applied | " +
                $"Tile ID: {tile.TileId} | " +
                $"Color: {tile.ColorType} | " +
                $"Bonus: x{colorBonusMultiplier} | " +
                $"Score: {oldScore} -> {currentScore}"
            );

            return true;
        }

        private bool IsPositiveOperation(TileOperationType operationType)
        {
            return operationType == TileOperationType.PlusOne ||
                   operationType == TileOperationType.PlusFive ||
                   operationType == TileOperationType.PlusTen;
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
                UpdateComboFromTile(path[i]);
                TryApplyColorBonus(path[i]);
            }

            Debug.Log($"Final Score Calculated | Score: {currentScore}");
        }
    }
}