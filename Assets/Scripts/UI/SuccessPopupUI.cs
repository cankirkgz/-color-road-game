using DG.Tweening;
using RenkYolu.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RenkYolu.UI
{
    public class SuccessPopupUI : MonoBehaviour
    {
        public static SuccessPopupUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupPanel;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text bonusText;
        [SerializeField] private TMP_Text totalScoreText;
        [SerializeField] private TMP_Text starsText;

        [Header("Buttons")]
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button replayButton;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.18f;
        [SerializeField] private float scaleDuration = 0.24f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(HandleNextLevelButtonClicked);
            }

            if (replayButton != null)
            {
                replayButton.onClick.AddListener(HandleReplayButtonClicked);
            }

            HideInstant();
        }

        private void OnDestroy()
        {
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(HandleNextLevelButtonClicked);
            }

            if (replayButton != null)
            {
                replayButton.onClick.RemoveListener(HandleReplayButtonClicked);
            }
        }

        public void ShowSuccess(int finalScore, int bonusScore, int totalScore)
        {
            if (panelRoot == null || canvasGroup == null || popupPanel == null)
            {
                Debug.LogError("SuccessPopupUI references are missing.");
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
                titleText.text = "LEVEL COMPLETE";
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore}";
            }

            if (bonusText != null)
            {
                bonusText.text = $"Bonus: +{bonusScore}";
            }

            if (totalScoreText != null)
            {
                totalScoreText.text = $"Total: {totalScore}";
            }

            if (starsText != null)
            {
                starsText.text = "* * *";
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

            Debug.Log("Success Popup Shown");
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

        private void HandleNextLevelButtonClicked()
        {
            Debug.Log("Next Level button clicked. Level system will be added later.");
        }

        private void HandleReplayButtonClicked()
        {
            Debug.Log("Replay button clicked.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
            else
            {
                Debug.LogError("GameManager is missing. Cannot restart level.");
            }
        }
    }
}