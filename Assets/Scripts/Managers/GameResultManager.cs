using RenkYolu.Core;
using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Managers
{
    public class GameResultManager : MonoBehaviour
    {
        public static GameResultManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool logResultDetails = true;

        private Tile lastFinishTile;

        public Tile LastFinishTile => lastFinishTile;

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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Result);
            }
            else
            {
                Debug.LogError("GameManager is missing. Cannot enter Result state.");
                return;
            }

            if (finishTile == null)
            {
                Debug.LogWarning("Result evaluation started, but finish tile is null.");
                return;
            }

            if (ScoreManager.Instance == null)
            {
                Debug.LogWarning("ScoreManager is missing. Start color cannot be read.");
                return;
            }

            if (!ScoreManager.Instance.HasStartColor)
            {
                Debug.LogWarning("Result evaluation started, but start color is not set.");
                return;
            }

            if (logResultDetails)
            {
                Debug.Log(
                    $"Result Evaluation Started | " +
                    $"Start Color: {ScoreManager.Instance.StartColor} | " +
                    $"Finish Tile ID: {finishTile.TileId} | " +
                    $"Finish Color: {finishTile.ColorType} | " +
                    $"Current Score: {ScoreManager.Instance.CurrentScore}"
                );
            }

            Debug.Log("Start color is known. Finish color check will be added on Day 3.");
        }
    }
}