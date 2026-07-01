using System.Collections.Generic;
using RenkYolu.Grid;
using UnityEngine;
using RenkYolu.Player;
using RenkYolu.Managers;
using RenkYolu.InputSystem;

namespace RenkYolu.Levels
{
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GridManager gridManager;

        [Header("Level Settings")]
        [SerializeField] private LevelData startingLevel;
        [SerializeField] private List<LevelData> levels = new List<LevelData>();
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PathInputManager pathInputManager;

        private int currentLevelIndex;

        public LevelData CurrentLevelData { get; private set; }

        public void ReloadCurrentLevel()
        {
            if (CurrentLevelData == null)
            {
                Debug.LogWarning("Current LevelData is missing. Cannot reload current level.");
                return;
            }

            LoadLevel(CurrentLevelData);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            LoadStartingLevel();
        }

        private void LoadStartingLevel()
        {
            if (levels != null && levels.Count > 0)
            {
                currentLevelIndex = GetStartingLevelIndex();
                LoadLevel(levels[currentLevelIndex]);
                return;
            }

            if (startingLevel == null)
            {
                Debug.LogError("Starting LevelData is missing on LevelLoader!");
                return;
            }

            currentLevelIndex = 0;
            LoadLevel(startingLevel);
        }

        public void LoadLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                Debug.LogError("Cannot load level. LevelData is null.");
                return;
            }

            if (gridManager == null)
            {
                Debug.LogError("GridManager reference is missing on LevelLoader!");
                return;
            }

            CurrentLevelData = levelData;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SaveLevelStartScore();
            }

            Debug.Log(
                $"Loading Level | " +
                $"ID: {levelData.LevelId} | " +
                $"Name: {levelData.LevelName}"
            );

            if (pathInputManager != null)
            {
                pathInputManager.ClearPath();
            }
            else
            {
                Debug.LogWarning("PathInputManager reference is missing on LevelLoader!");
            }

            gridManager.LoadLevel(levelData);

            if (playerController != null)
            {
                playerController.SpawnAtStartTile();
            }
            else
            {
                Debug.LogWarning("PlayerController reference is missing on LevelLoader!");
            }
        }

        public void LoadNextLevel()
        {
            if (levels == null || levels.Count == 0)
            {
                Debug.LogWarning("Level list is empty. Cannot load next level.");
                return;
            }

            if (currentLevelIndex >= levels.Count - 1)
            {
                Debug.Log("Last level completed. No next level available.");
                return;
            }

            currentLevelIndex++;

            LoadLevel(levels[currentLevelIndex]);
        }

        public bool HasNextLevel()
        {
            return levels != null && levels.Count > 0 && currentLevelIndex < levels.Count - 1;
        }

        public int GetLevelCount()
        {
            return levels != null
                ? levels.Count
                : 0;
        }

        public int GetCurrentLevelNumber()
        {
            return currentLevelIndex + 1;
        }

        private int GetStartingLevelIndex()
        {
            int unlockedLevel = LevelProgressManager.GetUnlockedLevel();

            if (levels == null || levels.Count == 0)
            {
                return 0;
            }

            int unlockedLevelIndex = Mathf.Clamp(
                unlockedLevel - 1,
                0,
                levels.Count - 1
            );

            Debug.Log(
                $"Starting Level Loaded From Progress | " +
                $"Unlocked Level: {unlockedLevel} | " +
                $"Starting Index: {unlockedLevelIndex}"
            );

            return unlockedLevelIndex;
        }
    }
}