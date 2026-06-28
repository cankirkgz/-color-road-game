using RenkYolu.Core;
using RenkYolu.Grid;
using UnityEngine;
using RenkYolu.UI;
using RenkYolu.Levels;
using RenkYolu.InputSystem;

namespace RenkYolu.Managers
{
    public class GameResultManager : MonoBehaviour
    {
        public static GameResultManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool logResultDetails = true;

        [Header("References")]
        [SerializeField] private LevelLoader levelLoader;
        [SerializeField] private PathInputManager pathInputManager;

        private Tile lastFinishTile;
        private bool lastResultWasSuccess;

        public Tile LastFinishTile => lastFinishTile;
        public bool LastResultWasSuccess => lastResultWasSuccess;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void EvaluateResult(Tile finishTile)
        {
            lastFinishTile = finishTile;
            lastResultWasSuccess = false;

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing. Cannot evaluate result.");
                return;
            }

            GameManager.Instance.ChangeState(GameState.Result);

            if (finishTile == null)
            {
                Debug.LogWarning("Result evaluation failed. Finish tile is null.");
                HandleFail("Finish tile is null.");
                return;
            }

            if (ScoreManager.Instance == null)
            {
                Debug.LogWarning("Result evaluation failed. ScoreManager is missing.");
                HandleFail("ScoreManager is missing.");
                return;
            }

            if (!ScoreManager.Instance.HasStartColor)
            {
                Debug.LogWarning("Result evaluation failed. Start color is not set.");
                HandleFail("Start color is not set.");
                return;
            }

            TileColorType startColor = ScoreManager.Instance.StartColor;
            TileColorType finishColor = finishTile.ColorType;

            bool isSuccess = finishColor == startColor;

            if (logResultDetails)
            {
                Debug.Log(
                    $"Result Evaluation Started | " +
                    $"Start Color: {startColor} | " +
                    $"Finish Tile ID: {finishTile.TileId} | " +
                    $"Finish Color: {finishColor} | " +
                    $"Current Score: {ScoreManager.Instance.CurrentScore}"
                );
            }

            if (isSuccess)
            {
                HandleSuccess(startColor, finishColor);
            }
            else
            {
                HandleFail(startColor, finishColor);
            }
        }

        private void HandleSuccess(TileColorType startColor, TileColorType finishColor)
        {
            lastResultWasSuccess = true;

            GameManager.Instance.ChangeState(GameState.LevelComplete);

            int finalScore = ScoreManager.Instance != null
                ? ScoreManager.Instance.CurrentScore
                : 0;

            int bonusScore = 0;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.CommitCurrentLevelScore();
            }

            int totalScore = ScoreManager.Instance != null
                ? ScoreManager.Instance.TotalScore
                : finalScore + bonusScore;

            Debug.Log(
                $"SUCCESS | " +
                $"Start Color: {startColor} | " +
                $"Finish Color: {finishColor} | " +
                $"Final Score: {finalScore} | " +
                $"Bonus: {bonusScore} | " +
                $"Total: {totalScore}"
            );

            if (SuccessPopupUI.Instance != null)
            {
                SuccessPopupUI.Instance.ShowSuccess(finalScore, bonusScore, totalScore);
            }
            else
            {
                Debug.LogWarning("SuccessPopupUI is missing. Success popup cannot be shown.");
            }
        }

        private void HandleFail(TileColorType startColor, TileColorType finishColor)
        {
            lastResultWasSuccess = false;

            GameManager.Instance.ChangeState(GameState.LevelFailed);

            int finalScore = ScoreManager.Instance != null
                ? ScoreManager.Instance.CurrentScore
                : 0;

            Debug.Log(
                $"FAIL | Wrong Finish Color | " +
                $"Start Color: {startColor} | " +
                $"Finish Color: {finishColor} | " +
                $"Final Score: {finalScore}"
            );

            if (FailPopupUI.Instance != null)
            {
                FailPopupUI.Instance.ShowFail(startColor, finishColor, finalScore);
            }
            else
            {
                Debug.LogWarning("FailPopupUI is missing. Fail popup cannot be shown.");
            }
        }

        private void HandleFail(string reason)
        {
            lastResultWasSuccess = false;

            GameManager.Instance.ChangeState(GameState.LevelFailed);

            Debug.Log($"FAIL | Reason: {reason}");
        }

        public void OnNextLevelButtonClicked()
        {
            if (!lastResultWasSuccess)
            {
                Debug.LogWarning("Next Level button clicked but last result was not success.");
                return;
            }

            LevelLoader loader = levelLoader != null
                ? levelLoader
                : LevelLoader.Instance;

            if (loader == null)
            {
                Debug.LogError("LevelLoader is missing. Cannot load next level.");
                return;
            }

            if (!loader.HasNextLevel())
            {
                Debug.Log("There is no next level.");
                return;
            }

            if (pathInputManager != null)
            {
                pathInputManager.ClearPath();
            }

            loader.LoadNextLevel();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevelFlow();
            }
            else
            {
                Debug.LogError("GameManager is missing. Cannot restart level flow.");
            }
        }
    }
}