using RenkYolu.Core;
using RenkYolu.Grid;
using UnityEngine;
using RenkYolu.UI;

namespace RenkYolu.Managers
{
    public class GameResultManager : MonoBehaviour
    {
        public static GameResultManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool logResultDetails = true;

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

            Debug.Log(
                $"SUCCESS | " +
                $"Start Color: {startColor} | " +
                $"Finish Color: {finishColor} | " +
                $"Final Score: {ScoreManager.Instance?.CurrentScore}"
            );
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
    }
}