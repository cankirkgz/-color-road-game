using RenkYolu.Levels;
using TMPro;
using UnityEngine;

namespace RenkYolu.UI
{
    public class LevelMaxScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text maxScoreText;

        private LevelData lastLevelData;

        private void Update()
        {
            if (LevelLoader.Instance == null)
            {
                return;
            }

            LevelData currentLevelData = LevelLoader.Instance.CurrentLevelData;

            if (currentLevelData == null || currentLevelData == lastLevelData)
            {
                return;
            }

            lastLevelData = currentLevelData;

            int maxScore = LevelStarCalculator.CalculateMaxPossibleScore(currentLevelData);

            if (maxScoreText != null)
            {
                maxScoreText.text = $"Max Score: {maxScore}";
            }

            Debug.Log($"Level Max Score | Level: {currentLevelData.LevelId} | Max Score: {maxScore}");
        }
    }
}