using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using RenkYolu.Grid;
using RenkYolu.UI;

namespace RenkYolu.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField]
        private RenkYolu.Core.GameState currentState;
        public RenkYolu.Core.GameState CurrentState => currentState;

        [SerializeField] private GridManager gridManager;
        [SerializeField] private float memorizeDuration = 5f;
        [SerializeField] private GameFlowUI gameFlowUI;

        private Coroutine levelFlowCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // DontDestroyOnLoad(gameObject);

            Debug.Log("Game Manager Initialized");
        }

        private void Start()
        {
            RestartLevelFlow();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                LevelProgressManager.ResetProgress();

                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.ResetPersistentScores();
                }

                Debug.Log("DEBUG | Progress and scores reset with P key.");
            }
        }

        public void RestartLevelFlow()
        {
            Time.timeScale = 1f;

            if (levelFlowCoroutine != null)
            {
                StopCoroutine(levelFlowCoroutine);
            }

            levelFlowCoroutine = StartCoroutine(StartLevelFlow());
        }

        private IEnumerator StartLevelFlow()
        {
            ChangeState(RenkYolu.Core.GameState.Memorizing);

            if (gameFlowUI != null)
            {
                gameFlowUI.HideConfirmButton();
            }

            float remainingTime = memorizeDuration;

            while (remainingTime > 0f)
            {
                if (gameFlowUI != null)
                {
                    gameFlowUI.ShowMemorizing(remainingTime);
                }

                remainingTime -= Time.deltaTime;

                yield return null;
            }

            ChangeState(RenkYolu.Core.GameState.Hidden);

            if (gridManager != null)
            {
                gridManager.HideAllTiles();
            }
            else
            {
                Debug.LogError("GridManager reference is missing on GameManager!");
            }

            ChangeState(RenkYolu.Core.GameState.Drawing);

            if (gameFlowUI != null)
            {
                gameFlowUI.ShowConfirmButton();
            }

            if (gameFlowUI != null)
            {
                gameFlowUI.ShowDrawing();
                gameFlowUI.HideAfterDelay(1f);
            }
        }

        public void ChangeState(RenkYolu.Core.GameState newState)
        {
            currentState = newState;

            Debug.Log($"STATE CHANGED -> {newState}");
        }

        public void RestartCurrentLevel()
        {
            Time.timeScale = 1f;

            Debug.Log("Restarting current level without scene reload.");

            if (FailPopupUI.Instance != null)
            {
                FailPopupUI.Instance.HideInstant();
            }

            if (SuccessPopupUI.Instance != null)
            {
                SuccessPopupUI.Instance.HideInstant();
            }
            
            if (RenkYolu.Levels.LevelLoader.Instance != null)
            {
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.RestoreLevelStartScore();
                }

                RenkYolu.Levels.LevelLoader.Instance.ReloadCurrentLevel();
                RestartLevelFlow();
            }
            else
            {
                Debug.LogError("LevelLoader is missing. Cannot restart current level.");
            }
        }
    }
}