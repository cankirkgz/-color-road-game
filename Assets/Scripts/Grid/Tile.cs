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
        [SerializeField] private Color wallColor = new Color(0.12f, 0.12f, 0.12f);
        [SerializeField] private Color blockedFeedbackColor = new Color(0.6f, 0.05f, 0.05f);
        [SerializeField] private float blockedFeedbackDuration = 0.08f;

        [Header("Special Tile Visuals")]
        [SerializeField] private TMP_Text spikeIconText;
        [SerializeField] private Color spikeIconColor = new Color(1f, 0.18f, 0.18f);
        [SerializeField] private Color spikeFlashColor = new Color(1f, 0.05f, 0.05f);
        [SerializeField] private float spikeEffectDuration = 0.08f;
        [SerializeField] private float spikePulseScale = 1.18f;

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
        public TileSpecialType SpecialType => tileData.specialType;
        public GridDirection SpikeDirection => tileData.spikeDirection;
        public bool IsWall => SpecialType == TileSpecialType.Wall;
        public bool IsSpike => SpecialType == TileSpecialType.Spike;
        public bool IsWalkable => tileData.isWalkable && !IsWall;
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
                true,
                TileSpecialType.None,
                GridDirection.None
            );

            if (IsWall)
            {
                ApplyWallVisuals();
            }
            else if (IsSpike)
            {
                ApplySpikeVisuals();
            }
            else
            {
                ApplyColor();
                ApplyOperationText();

                originalColor = spriteRenderer.color;

                HideSpecialIcons();
            }

            isSelected = false;

            if (selectionBorder != null)
            {
                selectionBorder.SetActive(false);
            }

            gameObject.name = $"Tile_{TileId}_X{X}_Y{Y}";

            Debug.Log($"Tile Data Created | ID: {TileId}, X: {X}, Y: {Y}, Color: {ColorType}, Operation: {OperationType}, Value: {OperationValue}, Visible: {IsVisible}, Walkable: {IsWalkable}");
        }

        public void InitializeFromLevelData(
            int newTileId,
            int gridX,
            int gridY,
            TileColorType newColorType,
            TileOperationType newOperationType,
            int newOperationValue,
            bool walkable,
            TileSpecialType newSpecialType,
            GridDirection newSpikeDirection
        )
        {
            tileData = new TileData(
                newTileId,
                gridX,
                gridY,
                newColorType,
                newOperationType,
                newOperationValue,
                true,
                walkable,
                newSpecialType,
                newSpikeDirection
            );

            if (IsWall)
            {
                ApplyWallVisuals();
            }
            else if (IsSpike)
            {
                ApplySpikeVisuals();
            }
            else
            {
                ApplyColor();
                ApplyOperationText();

                originalColor = spriteRenderer.color;

                HideSpecialIcons();
            }

            isSelected = false;

            if (selectionBorder != null)
            {
                selectionBorder.SetActive(false);
            }

            gameObject.name = $"Tile_{TileId}_X{X}_Y{Y}";

            Debug.Log(
                $"Level Tile Data Created | " +
                $"ID: {TileId}, X: {X}, Y: {Y}, " +
                $"Color: {ColorType}, Operation: {OperationType}, " +
                $"Value: {OperationValue}, Visible: {IsVisible}, " +
                $"Walkable: {IsWalkable}, SpecialType: {SpecialType}, " +
                $"IsWall: {IsWall}, IsSpike: {IsSpike}, " +
                $"SpikeDirection: {SpikeDirection}"
            );
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

            if (IsWall)
            {
                ApplyWallVisuals();
                Debug.Log($"Wall Tile Reveal Ignored | ID: {TileId}, X: {X}, Y: {Y}");
                return;
            }

            if (IsSpike)
            {
                ApplySpikeVisuals();
                Debug.Log($"Spike Tile Reveal Ignored | ID: {TileId}, X: {X}, Y: {Y}, Direction: {SpikeDirection}");
                return;
            }

            HideSpecialIcons();

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

            if (IsWall)
            {
                ApplyWallVisuals();
                Debug.Log($"Wall Tile Hide Ignored | ID: {TileId}, X: {X}, Y: {Y}");
                return;
            }

            if (IsSpike)
            {
                ApplySpikeVisuals();
                Debug.Log($"Spike Tile Hide Ignored | ID: {TileId}, X: {X}, Y: {Y}, Direction: {SpikeDirection}");
                return;
            }

            HideSpecialIcons();

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

        private void ApplyWallVisuals()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = wallColor;
                originalColor = wallColor;
            }

            if (operationText != null)
            {
                operationText.gameObject.SetActive(false);
            }

            if (selectionBorder != null)
            {
                selectionBorder.SetActive(false);
            }

            HideSpecialIcons();
        }

        private void ApplySpikeVisuals()
        {
            if (spikeIconText == null)
            {
                Debug.LogWarning($"SpikeIconText reference is missing on Tile | ID: {TileId}, X: {X}, Y: {Y}");
                return;
            }

            spikeIconText.gameObject.SetActive(true);
            spikeIconText.text = GetSpikeDirectionIcon();
            spikeIconText.color = spikeIconColor;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.18f, 0.18f, 0.18f);
                originalColor = spriteRenderer.color;
            }

            if (operationText != null)
            {
                operationText.gameObject.SetActive(false);
            }

            Debug.Log(
                $"Spike Visual Applied | ID: {TileId}, X: {X}, Y: {Y}, " +
                $"Direction: {SpikeDirection}, Icon: {spikeIconText.text}"
            );
        }

        private string GetSpikeDirectionIcon()
        {
            switch (SpikeDirection)
            {
                case GridDirection.Up:
                    return "^";

                case GridDirection.Down:
                    return "v";

                case GridDirection.Left:
                    return "<";

                case GridDirection.Right:
                    return ">";

                default:
                    return "?";
            }
        }

        private void HideSpecialIcons()
        {
            if (spikeIconText != null)
            {
                spikeIconText.gameObject.SetActive(false);
            }
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

        public void PlayBlockedFeedback()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (colorAnimationCoroutine != null)
            {
                StopCoroutine(colorAnimationCoroutine);
                colorAnimationCoroutine = null;
            }

            spriteRenderer.DOKill();

            Color baseColor = GetCurrentTargetColor();

            Sequence blockedSequence = DOTween.Sequence();

            blockedSequence.Append(
                spriteRenderer.DOColor(blockedFeedbackColor, blockedFeedbackDuration)
                    .SetEase(Ease.OutQuad)
            );

            blockedSequence.Append(
                spriteRenderer.DOColor(baseColor, blockedFeedbackDuration)
                    .SetEase(Ease.InQuad)
            );

            blockedSequence.OnComplete(() =>
            {
                spriteRenderer.color = baseColor;
                originalColor = baseColor;
            });
        }

        public void PlayGlow()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (colorAnimationCoroutine != null)
            {
                StopCoroutine(colorAnimationCoroutine);
                colorAnimationCoroutine = null;
            }

            spriteRenderer.DOKill();

            Color baseColor = GetCurrentTargetColor();
            Color glowColor = Color.white;

            Sequence glowSequence = DOTween.Sequence();

            glowSequence.Append(
                spriteRenderer.DOColor(glowColor, 0.08f)
                    .SetEase(Ease.OutQuad)
            );

            glowSequence.Append(
                spriteRenderer.DOColor(baseColor, 0.18f)
                    .SetEase(Ease.InQuad)
            );

            glowSequence.OnComplete(() =>
            {
                spriteRenderer.color = baseColor;
                originalColor = baseColor;
            });
        }

        public void PlaySpikeEffect()
        {
            if (!IsSpike)
            {
                return;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.DOKill();

                Color baseColor = GetCurrentTargetColor();
                Sequence colorSequence = DOTween.Sequence();

                colorSequence.Append(
                    spriteRenderer.DOColor(spikeFlashColor, spikeEffectDuration)
                        .SetEase(Ease.OutQuad)
                );
                colorSequence.Append(
                    spriteRenderer.DOColor(baseColor, spikeEffectDuration)
                        .SetEase(Ease.InQuad)
                );
                colorSequence.OnComplete(() =>
                {
                    spriteRenderer.color = baseColor;
                    originalColor = baseColor;
                });
            }

            if (spikeIconText != null)
            {
                spikeIconText.transform.DOKill();

                Vector3 originalScale = Vector3.one;
                spikeIconText.transform.localScale = originalScale;

                Sequence iconSequence = DOTween.Sequence();

                iconSequence.Append(
                    spikeIconText.transform.DOScale(originalScale * spikePulseScale, spikeEffectDuration)
                        .SetEase(Ease.OutBack)
                );
                iconSequence.Append(
                    spikeIconText.transform.DOScale(originalScale, spikeEffectDuration)
                        .SetEase(Ease.InBack)
                );
            }

            transform.DOKill();

            Vector3 tileOriginalScale = transform.localScale;
            Vector3 targetScale = tileOriginalScale * 1.04f;
            Sequence tileSequence = DOTween.Sequence();

            tileSequence.Append(
                transform.DOScale(targetScale, spikeEffectDuration)
                    .SetEase(Ease.OutQuad)
            );
            tileSequence.Append(
                transform.DOScale(tileOriginalScale, spikeEffectDuration)
                    .SetEase(Ease.InQuad)
            );

            Debug.Log($"Spike Effect Played | X:{X}, Y:{Y}, Direction:{SpikeDirection}");
        }

        private Color GetCurrentTargetColor()
        {
            if (tileData == null)
            {
                return Color.white;
            }

            if (IsWall)
            {
                return wallColor;
            }

            if (IsSpike)
            {
                return new Color(0.18f, 0.18f, 0.18f);
            }

            if (tileData.isVisible)
            {
                return GetTileColor();
            }

            return Color.gray;
        }
    }
}
