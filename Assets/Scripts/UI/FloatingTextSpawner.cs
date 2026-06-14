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

        [Header("Colors")]
        [SerializeField] private Color positiveColor = Color.green;
        [SerializeField] private Color negativeColor = Color.red;
        [SerializeField] private Color multiplierColor = Color.yellow;
        [SerializeField] private Color divideColor = Color.cyan;

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

            if (floatingTextPrefab == null)
            {
                Debug.LogWarning("Floating text prefab is missing.");
                return;
            }

            string textValue = GetOperationText(tile.OperationType, tile.OperationValue);
            Color textColor = GetOperationColor(tile.OperationType);

            Vector3 spawnPosition = tile.transform.position + spawnOffset;

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