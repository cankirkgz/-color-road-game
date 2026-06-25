using RenkYolu.Grid.Data;
using TMPro;
using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace RenkYolu.Grid
{
    public class Tile : MonoBehaviour
    {
        [Header("Tile Info")]
        [SerializeField] private TileData tileData;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text operationText;
        [SerializeField] private GameObject selectionBorder;

        private Color originalColor;
        private bool isSelected;

        [SerializeField] private float hideAnimationDuration = 0.25f;
        [SerializeField] private float textFadeDuration = 0.2f;

        private Coroutine colorAnimationCoroutine;
        private Coroutine textAnimationCoroutine;

        public Tile UpNeighbour { get; private set; }
        public Tile DownNeighbour { get; private set; }
        public Tile LeftNeighbour { get; private set; }
        public Tile RightNeighbour { get; private set; }

        public int TileId => tileData.tileId;
        public int X => tileData.x;
        public int Y => tileData.y;
        public TileColorType ColorType => tileData.colorType;
        public TileOperationType OperationType => tileData.operationType;
        public int OperationValue => tileData.operationValue;
        public bool IsVisible => tileData.isVisible;
        public bool IsWalkable => tileData.isWalkable;
        public bool IsSelected => isSelected;

        public void Initialize(int newTileId, int gridX, int gridY, TileColorType newColorType, TileOperationType newOperationType)
        {
            TileOperationData operationData = TileDataLibrary.GetOperationData(newOperationType);

            int operationValue = 0;

            if (operationData != null)
            {
                operationValue = operationData.Value;
            }

            tileData = new TileData(
                newTileId,
                gridX,
                gridY,
                newColorType,
                newOperationType,
                operationValue,
                true,
                true
            );

            ApplyColor();
            ApplyOperationText();

            originalColor = spriteRenderer.color;
            isSelected = false;

            if (selectionBorder != null)
            {
                selectionBorder.SetActive(false);
            }

            gameObject.name = $"Tile_{TileId}_X{X}_Y{Y}";

            Debug.Log($"Tile Data Created | ID: {TileId}, X: {X}, Y: {Y}, Color: {ColorType}, Operation: {OperationType}, Value: {OperationValue}, Visible: {IsVisible}, Walkable: {IsWalkable}");
        }

        public void SetNeighbours(Tile up, Tile down, Tile left, Tile right)
        {
            UpNeighbour = up;
            DownNeighbour = down;
            LeftNeighbour = left;
            RightNeighbour = right;

            Debug.Log(
                $"Neighbours Set | Tile: X{X}, Y{Y} | " +
                $"Up: {GetNeighbourName(UpNeighbour)}, " +
                $"Down: {GetNeighbourName(DownNeighbour)}, " +
                $"Left: {GetNeighbourName(LeftNeighbour)}, " +
                $"Right: {GetNeighbourName(RightNeighbour)}"
            );
        }

        public bool IsNeighbour(Tile otherTile)
        {
            return otherTile == UpNeighbour ||
                otherTile == DownNeighbour ||
                otherTile == LeftNeighbour ||
                otherTile == RightNeighbour;
        }

        private string GetNeighbourName(Tile tile)
        {
            if (tile == null)
            {
                return "None";
            }

            return $"X{tile.X}_Y{tile.Y}";
        }

        public void Select()
        {
            if (isSelected)
            {
                return;
            }

            isSelected = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.black, 0.12f);
            }

            if (selectionBorder != null)
            {
                selectionBorder.SetActive(true);
            }

            Debug.Log($"Tile Selected | ID: {TileId}, X: {X}, Y: {Y}");
        }

        public void Deselect()
        {
            if (!isSelected)
            {
                return;
            }

            isSelected = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            if (selectionBorder != null)
            {
                selectionBorder.SetActive(false);
            }

            Debug.Log($"Tile Deselected | ID: {TileId}, X: {X}, Y: {Y}");
        }

        public void Reveal()
        {
            tileData.isVisible = true;

            ApplyOperationText();

            if (operationText != null)
            {
                operationText.gameObject.SetActive(true);

                if (textAnimationCoroutine != null)
                {
                    StopCoroutine(textAnimationCoroutine);
                }

                textAnimationCoroutine =
                    StartCoroutine(FadeText(0f, 1f, true));
            }

            if (colorAnimationCoroutine != null)
            {
                StopCoroutine(colorAnimationCoroutine);
            }

            Color targetColor = GetTileColor();

            colorAnimationCoroutine =
                StartCoroutine(AnimateColor(spriteRenderer.color, targetColor));

            Debug.Log($"Tile Revealed | ID: {TileId}, X: {X}, Y: {Y}");
        }

        public void Hide()
        {
            tileData.isVisible = false;

            if (operationText != null)
            {
                if (textAnimationCoroutine != null)
                {
                    StopCoroutine(textAnimationCoroutine);
                }

                textAnimationCoroutine =
                    StartCoroutine(FadeText(1f, 0f, false));
            }

            if (colorAnimationCoroutine != null)
            {
                StopCoroutine(colorAnimationCoroutine);
            }

            colorAnimationCoroutine = StartCoroutine(AnimateColor(spriteRenderer.color, Color.gray));

            Debug.Log($"Tile Hidden | ID: {TileId}, X: {X}, Y: {Y}");
        }

        private void ApplyColor()
        {
            if (spriteRenderer == null)
            {
                Debug.LogError("Sprite Renderer is missing on Tile!");
                return;
            }

            TileColorData colorData = TileDataLibrary.GetColorData(ColorType);

            if (colorData == null)
            {
                return;
            }

            spriteRenderer.color = colorData.Color;
        }

        private void ApplyOperationText()
        {
            if (operationText == null)
            {
                Debug.LogError("Operation Text is missing on Tile!");
                return;
            }

            TileOperationData operationData = TileDataLibrary.GetOperationData(OperationType);

            if (operationData == null)
            {
                return;
            }

            operationText.text = operationData.Label;
        }

        private IEnumerator AnimateColor(Color startColor, Color targetColor)
        {
            if (spriteRenderer == null)
            {
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < hideAnimationDuration)
            {
                elapsedTime += Time.deltaTime;

                float progress = elapsedTime / hideAnimationDuration;
                spriteRenderer.color = Color.Lerp(startColor, targetColor, progress);

                yield return null;
            }

            spriteRenderer.color = targetColor;
            originalColor = targetColor;
        }

        private IEnumerator FadeText(
            float startAlpha,
            float targetAlpha,
            bool keepVisible)
        {
            if (operationText == null)
            {
                yield break;
            }

            Color color = operationText.color;
            color.a = startAlpha;
            operationText.color = color;

            float elapsedTime = 0f;

            while (elapsedTime < textFadeDuration)
            {
                elapsedTime += Time.deltaTime;

                float progress = elapsedTime / textFadeDuration;

                color.a = Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress);

                operationText.color = color;

                yield return null;
            }

            color.a = targetAlpha;
            operationText.color = color;

            if (!keepVisible)
            {
                operationText.gameObject.SetActive(false);
            }
        }

        private Color GetTileColor()
        {
            TileColorData colorData = TileDataLibrary.GetColorData(ColorType);

            if (colorData == null)
            {
                return Color.white;
            }

            return colorData.Color;
        }

        public void PlayGlow()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            Color baseColor = spriteRenderer.color;
            Color glowColor = Color.white;

            spriteRenderer.DOKill();

            Sequence glowSequence = DOTween.Sequence();

            glowSequence.Append(
                spriteRenderer.DOColor(glowColor, 0.08f)
                    .SetEase(Ease.OutQuad)
            );

            glowSequence.Append(
                spriteRenderer.DOColor(baseColor, 0.18f)
                    .SetEase(Ease.InQuad)
            );
        }
    }
}