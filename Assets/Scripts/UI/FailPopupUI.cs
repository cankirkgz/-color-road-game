using DG.Tweening;
using RenkYolu.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RenkYolu.UI
{
    public class FailPopupUI : MonoBehaviour
    {
        public static FailPopupUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupPanel;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private TMP_Text scoreText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.18f;
        [SerializeField] private float scaleDuration = 0.22f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(HandleRetryButtonClicked);
            }

            HideInstant();
        }

        private void OnDestroy()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(HandleRetryButtonClicked);
            }
        }

        public void ShowFail(TileColorType startColor, TileColorType finishColor, int finalScore)
        {
            if (panelRoot == null || canvasGroup == null || popupPanel == null)
            {
                Debug.LogError("FailPopupUI references are missing.");
                return;
            }

            panelRoot.SetActive(true);

            canvasGroup.DOKill();
            popupPanel.DOKill();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            popupPanel.localScale = Vector3.zero;

            if (titleText != null)
            {
                titleText.text = "❌ Wrong Color";
            }

            if (detailText != null)
            {
                detailText.text = $"Start: {startColor}\nFinish: {finishColor}";
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score: {finalScore}";
            }

            Sequence sequence = DOTween.Sequence();

            sequence.Append(
                canvasGroup.DOFade(1f, fadeDuration)
                    .SetEase(Ease.OutQuad)
            );

            sequence.Join(
                popupPanel.DOScale(Vector3.one, scaleDuration)
                    .SetEase(Ease.OutBack)
            );

            Debug.Log("Fail Popup Shown");
        }

        public void HideInstant()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (popupPanel != null)
            {
                popupPanel.DOKill();
                popupPanel.localScale = Vector3.zero;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void HandleRetryButtonClicked()
        {
            Debug.Log("Retry button clicked. Retry system will be added on Day 6.");
        }
    }
}