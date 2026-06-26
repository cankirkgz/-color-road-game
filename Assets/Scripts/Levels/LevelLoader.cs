using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Levels
{
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GridManager gridManager;

        [Header("Level Settings")]
        [SerializeField] private LevelData startingLevel;

        public LevelData CurrentLevelData { get; private set; }

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
            if (startingLevel == null)
            {
                Debug.LogError("Starting LevelData is missing on LevelLoader!");
                return;
            }

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

            Debug.Log(
                $"Loading Level | " +
                $"ID: {levelData.LevelId} | " +
                $"Name: {levelData.LevelName}"
            );

            gridManager.LoadLevel(levelData);
        }
    }
}