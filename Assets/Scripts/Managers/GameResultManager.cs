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

            if (logResultDetails)
            {
                Debug.Log(
                    $"Result Evaluation Started | " +
                    $"Finish Tile ID: {finishTile.TileId} | " +
                    $"Finish Color: {finishTile.ColorType} | " +
                    $"Current Score: {ScoreManager.Instance?.CurrentScore}"
                );
            }

            Debug.Log("Result phase is ready. Success / Fail check will be added later.");
        }
    }
}