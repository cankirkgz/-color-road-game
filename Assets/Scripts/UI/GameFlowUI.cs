using TMPro;
using UnityEngine;
using System.Collections;

namespace RenkYolu.UI
{
    public class GameFlowUI : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private float pulseScale = 1.18f;
        [SerializeField] private float pulseDuration = 0.18f;
        [SerializeField] private GameObject confirmPathButton;

        private Coroutine pulseCoroutine;
        private int lastDisplayedSecond = -1;
        private Vector3 defaultScale;

        public void ShowMemorizing(float remainingTime)
        {
            if (phaseText == null)
            {
                return;
            }

            phaseText.gameObject.SetActive(true);
            phaseText.text = $"HARİTAYI EZBERLE\n{Mathf.CeilToInt(remainingTime)}";

            int displayedSecond = Mathf.CeilToInt(remainingTime);

            if (displayedSecond <= 3 && displayedSecond != lastDisplayedSecond)
            {
                lastDisplayedSecond = displayedSecond;
                PlayPulse();
            }
        }

        private void Start()
        {
            if (phaseText != null)
            {
                defaultScale = phaseText.transform.localScale;
            }
        }

        public void ShowDrawing()
        {
            phaseText.transform.localScale = defaultScale;
            
            if (phaseText == null)
            {
                return;
            }

            phaseText.gameObject.SetActive(true);
            phaseText.text = "ÇİZİME BAŞLA";
        }

        public void Hide()
        {
            if (phaseText == null)
            {
                return;
            }

            phaseText.gameObject.SetActive(false);
        }

        public void HideAfterDelay(float delay)
        {
            Invoke(nameof(Hide), delay);
        }

        public void ShowConfirmButton()
        {
            if (confirmPathButton != null)
            {
                confirmPathButton.SetActive(true);
            }
        }

        public void HideConfirmButton()
        {
            if (confirmPathButton != null)
            {
                confirmPathButton.SetActive(false);
            }
        }

        private void PlayPulse()
        {
            if (phaseText == null)
            {
                return;
            }

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }

            pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            float elapsedTime = 0f;

            while (elapsedTime < pulseDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / pulseDuration;
                float scale = Mathf.Lerp(1f, pulseScale, progress);

                phaseText.transform.localScale = defaultScale * scale;

                yield return null;
            }

            elapsedTime = 0f;

            while (elapsedTime < pulseDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / pulseDuration;
                float scale = Mathf.Lerp(pulseScale, 1f, progress);

                phaseText.transform.localScale = defaultScale * scale;

                yield return null;
            }

            phaseText.transform.localScale = defaultScale;
        }
    }
}