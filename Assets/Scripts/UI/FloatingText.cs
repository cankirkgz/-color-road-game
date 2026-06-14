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

        public void Initialize(string value, Color accentColor)
        {
            if (text == null)
            {
                Debug.LogError("FloatingText TMP_Text reference is missing.");
                return;
            }

            text.text = value;
            text.color = defaultTextColor;

            if (shadowText != null)
            {
                shadowText.text = value;
                shadowText.color = shadowColor;
            }

            PlayAnimation();
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