using DG.Tweening;
using TMPro;
using UnityEngine;

namespace RenkYolu.UI
{
    public class FloatingText : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private TMP_Text shadowText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float moveDistance = 0.9f;
        [SerializeField] private float duration = 1.15f;

        [Header("Text Style")]
        [SerializeField] private Color defaultTextColor = Color.white;
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.75f);

        [Header("Font Sizes")]
        [SerializeField] private float operationFontSize = 28f;
        [SerializeField] private float bonusFontSize = 30f;
        [SerializeField] private float comboFontSize = 30f;
        [SerializeField] private float longTextFontSize = 24f;

        public void Initialize(string value, Color accentColor)
        {
            if (text == null)
            {
                Debug.LogError("FloatingText TMP_Text reference is missing.");
                return;
            }

            float targetFontSize = GetFontSize(value);

            text.text = value;
            text.color = defaultTextColor;
            text.fontSize = targetFontSize;

            if (shadowText != null)
            {
                shadowText.text = value;
                shadowText.color = shadowColor;
                shadowText.fontSize = targetFontSize;
            }

            PlayAnimation();
        }

        private float GetFontSize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return operationFontSize;
            }

            if (value.Length >= 8)
            {
                return longTextFontSize;
            }

            if (value.Contains("COMBO"))
            {
                return comboFontSize;
            }

            if (value.Contains("BONUS"))
            {
                return bonusFontSize;
            }

            return operationFontSize;
        }

        private void PlayAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            Vector3 targetPosition = transform.position + Vector3.up * moveDistance;

            Sequence sequence = DOTween.Sequence();

            sequence.Join(
                transform.DOMove(targetPosition, duration)
                    .SetEase(Ease.OutQuad)
            );

            if (canvasGroup != null)
            {
                sequence.Join(
                    canvasGroup.DOFade(0f, duration)
                        .SetEase(Ease.InQuad)
                );
            }

            sequence.OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}