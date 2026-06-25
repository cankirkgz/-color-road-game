using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.UI
{
    public class FloatingTextSpawner : MonoBehaviour
    {
        public static FloatingTextSpawner Instance { get; private set; }

        [Header("References")]
        [SerializeField] private FloatingText floatingTextPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.35f, -1f);
        [SerializeField] private Vector3 bonusSpawnOffset = new Vector3(0f, 0.75f, -1f);
        [SerializeField] private Vector3 comboSpawnOffset = new Vector3(0f, 1.05f, -1f);

        [Header("Colors")]
        [SerializeField] private Color positiveColor = Color.green;
        [SerializeField] private Color negativeColor = Color.red;
        [SerializeField] private Color multiplierColor = Color.yellow;
        [SerializeField] private Color divideColor = Color.cyan;
        [SerializeField] private Color bonusColor = new Color(1f, 0.85f, 0.1f, 1f);
        [SerializeField] private Color comboColor = new Color(1f, 0.45f, 1f, 1f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ShowTileOperation(Tile tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("Floating text skipped. Tile is null.");
                return;
            }

            string textValue = GetOperationText(tile.OperationType, tile.OperationValue);
            Color textColor = GetOperationColor(tile.OperationType);

            ShowText(textValue, tile.transform.position + spawnOffset, textColor);
        }

        public void ShowColorBonus(Tile tile, int multiplier)
        {
            if (tile == null)
            {
                Debug.LogWarning("Bonus floating text skipped. Tile is null.");
                return;
            }

            ShowText($"BONUS x{multiplier}", tile.transform.position + bonusSpawnOffset, bonusColor);
        }

        public void ShowCombo(Tile tile, int comboValue)
        {
            if (tile == null)
            {
                Debug.LogWarning("Combo floating text skipped. Tile is null.");
                return;
            }

            ShowText($"COMBO x{comboValue}", tile.transform.position + comboSpawnOffset, comboColor);
        }

        private void ShowText(string textValue, Vector3 spawnPosition, Color textColor)
        {
            if (floatingTextPrefab == null)
            {
                Debug.LogWarning("Floating text prefab is missing.");
                return;
            }

            FloatingText floatingText = Instantiate(
                floatingTextPrefab,
                spawnPosition,
                Quaternion.identity
            );

            floatingText.Initialize(textValue, textColor);
        }

        private string GetOperationText(TileOperationType operationType, int operationValue)
        {
            switch (operationType)
            {
                case TileOperationType.PlusOne:
                case TileOperationType.PlusFive:
                case TileOperationType.PlusTen:
                    return $"+{operationValue}";

                case TileOperationType.MinusFive:
                case TileOperationType.MinusTen:
                case TileOperationType.MinusFifteen:
                    return operationValue.ToString();

                case TileOperationType.MultiplyTwo:
                case TileOperationType.MultiplyThree:
                    return $"x{operationValue}";

                case TileOperationType.DivideTwo:
                    return $"÷{operationValue}";

                default:
                    return operationValue.ToString();
            }
        }

        private Color GetOperationColor(TileOperationType operationType)
        {
            switch (operationType)
            {
                case TileOperationType.PlusOne:
                case TileOperationType.PlusFive:
                case TileOperationType.PlusTen:
                    return positiveColor;

                case TileOperationType.MinusFive:
                case TileOperationType.MinusTen:
                case TileOperationType.MinusFifteen:
                    return negativeColor;

                case TileOperationType.MultiplyTwo:
                case TileOperationType.MultiplyThree:
                    return multiplierColor;

                case TileOperationType.DivideTwo:
                    return divideColor;

                default:
                    return Color.white;
            }
        }
    }
}