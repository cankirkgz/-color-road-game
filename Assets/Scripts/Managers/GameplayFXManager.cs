using DG.Tweening;
using RenkYolu.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace RenkYolu.Managers
{
    public class GameplayFXManager : MonoBehaviour
    {
        public static GameplayFXManager Instance { get; private set; }

        [Header("Screen Flash")]
        [SerializeField] private Image screenFlashImage;
        [SerializeField] private CanvasGroup screenFlashCanvasGroup;
        [SerializeField] private float flashDuration = 0.18f;
        [SerializeField] private float flashMaxAlpha = 0.18f;

        [Header("Player Punch")]
        [SerializeField] private float punchScaleAmount = 0.14f;
        [SerializeField] private float punchDuration = 0.16f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (screenFlashCanvasGroup != null)
            {
                screenFlashCanvasGroup.alpha = 0f;
            }

            if (screenFlashImage != null)
            {
                Color color = screenFlashImage.color;
                color.a = 1f;
                screenFlashImage.color = color;
            }
        }

        public void PlayComboFx(Tile tile, Transform playerTransform)
        {
            PlayScreenFlash();
            PlayTileGlow(tile);
            PlayPlayerPunch(playerTransform);

            Debug.Log("Combo FX Played");
        }

        public void PlayBonusFx(Tile tile, Transform playerTransform)
        {
            PlayScreenFlash();
            PlayTileGlow(tile);
            PlayPlayerPunch(playerTransform);

            Debug.Log("Bonus FX Played");
        }

        public void PlayCriticalFx(Tile tile, Transform playerTransform)
        {
            PlayScreenFlash();
            PlayTileGlow(tile);
            PlayPlayerPunch(playerTransform);

            Debug.Log("Critical FX Played");
        }

        private void PlayScreenFlash()
        {
            if (screenFlashCanvasGroup == null)
            {
                return;
            }

            screenFlashCanvasGroup.DOKill();
            screenFlashCanvasGroup.alpha = 0f;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(
                screenFlashCanvasGroup.DOFade(flashMaxAlpha, flashDuration * 0.4f)
                    .SetEase(Ease.OutQuad)
            );

            sequence.Append(
                screenFlashCanvasGroup.DOFade(0f, flashDuration * 0.6f)
                    .SetEase(Ease.InQuad)
            );
        }

        private void PlayTileGlow(Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            tile.PlayGlow();
        }

        private void PlayPlayerPunch(Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            playerTransform.DOKill();

            Vector3 originalScale = playerTransform.localScale;
            Vector3 punchScale = originalScale * (1f + punchScaleAmount);

            Sequence sequence = DOTween.Sequence();

            sequence.Append(
                playerTransform.DOScale(punchScale, punchDuration * 0.45f)
                    .SetEase(Ease.OutBack)
            );

            sequence.Append(
                playerTransform.DOScale(originalScale, punchDuration * 0.55f)
                    .SetEase(Ease.InOutQuad)
            );
        }
    }
}