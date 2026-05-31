using System.Collections;
using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Spawn Settings")]
        [SerializeField] private float zPosition = 0f;
        [SerializeField] private float sizeMultiplier = 0.55f;

        private Tile currentTile;

        public Tile CurrentTile => currentTile;

        private void Start()
        {
            StartCoroutine(SpawnWhenGridReady());
        }

        private IEnumerator SpawnWhenGridReady()
        {
            yield return new WaitUntil(() =>
                gridManager != null &&
                gridManager.HasGenerated &&
                gridManager.GetStartTile() != null);

            SpawnAtTile(gridManager.GetStartTile());
        }

        private void SpawnAtTile(Tile tile)
        {
            currentTile = tile;

            Vector3 spawnPosition = tile.transform.position;
            spawnPosition.z = zPosition;

            transform.position = spawnPosition;

            float playerSize = gridManager.TileSize * sizeMultiplier;
            transform.localScale = Vector3.one * playerSize;

            Debug.Log($"Player Spawned | Tile X: {tile.X}, Y: {tile.Y}");
        }
    }
}