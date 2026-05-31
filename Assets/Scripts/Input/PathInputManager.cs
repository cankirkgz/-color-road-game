using System.Collections.Generic;
using RenkYolu.Grid;
using RenkYolu.Managers;
using UnityEngine;
using RenkYolu.UI;
using RenkYolu.Player;

namespace RenkYolu.InputSystem
{
    public class PathInputManager : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera mainCamera;

        [Header("UI")]
        [SerializeField] private GameFlowUI gameFlowUI;

        [Header("Path Visual")]
        [SerializeField] private LineRenderer pathLineRenderer;
        [SerializeField] private float pathLineZPosition = -0.1f;

        [Header("Player")]
        [SerializeField] private PlayerMovement playerMovement;

        private readonly List<Tile> selectedPath = new List<Tile>();
        public IReadOnlyList<Tile> CurrentPath => selectedPath;
        private bool isDrawing;
        private bool startedWithExistingPath;
        private bool pathChangedDuringDrag;
        private Tile pointerStartTile;
        private Tile lastHandledTile;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is missing on PathInputManager!");
            }

            if (pathLineRenderer != null)
            {
                pathLineRenderer.positionCount = 0;
                pathLineRenderer.startWidth = 0.08f;
                pathLineRenderer.endWidth = 0.08f;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != RenkYolu.Core.GameState.Drawing)
            {
                return;
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                StartDrawing();
            }

            if (UnityEngine.Input.GetMouseButton(0) && isDrawing)
            {
                TryHandleTileUnderPointer();
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                FinishDrawing();
            }
        }

        private void StartDrawing()
        {
            Tile startTile = GetTileUnderPointer();

            if (startTile == null)
            {
                return;
            }

            isDrawing = true;
            startedWithExistingPath = selectedPath.Count > 0;
            pathChangedDuringDrag = false;
            pointerStartTile = startTile;
            lastHandledTile = null;

            HandleTile(startTile);
            lastHandledTile = startTile;
        }

        private void FinishDrawing()
        {
            if (!isDrawing)
            {
                return;
            }

            if (!pathChangedDuringDrag && CanTapUndoLastTile())
            {
                RemoveLastTileFromPath();
            }

            isDrawing = false;
            startedWithExistingPath = false;
            pathChangedDuringDrag = false;
            pointerStartTile = null;
            lastHandledTile = null;

            if (selectedPath.Count == 0)
            {
                Debug.LogWarning("Path is empty. Walking phase will not start.");
                return;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.CalculateScoreFromPath(selectedPath);
            }
            else
            {
                Debug.LogError("ScoreManager is missing in the scene!");
            }

            //GameManager.Instance.ChangeState(RenkYolu.Core.GameState.Walking);

            Debug.Log($"Path Finished | Tile Count: {selectedPath.Count}");
        }

        private void TryHandleTileUnderPointer()
        {
            Tile tile = GetTileUnderPointer();

            if (tile == null)
            {
                lastHandledTile = null;
                return;
            }

            if (tile == lastHandledTile)
            {
                return;
            }

            HandleTile(tile);
            lastHandledTile = tile;
        }

        private void HandleTile(Tile tile)
        {
            if (selectedPath.Count == 0)
            {
                AddTileToPath(tile);
                return;
            }

            int pathIndex = selectedPath.IndexOf(tile);

            if (pathIndex >= 0)
            {
                HandleSelectedTile(pathIndex);
                return;
            }

            TryAddTileToPath(tile);
        }

        private void HandleSelectedTile(int pathIndex)
        {
            int lastIndex = selectedPath.Count - 1;

            if (pathIndex == lastIndex)
            {
                return;
            }

            if (pathIndex == 0)
            {
                RemoveTilesFromIndex(0);
                return;
            }

            RemoveTilesAfterIndex(pathIndex);
        }

        private void TryAddTileToPath(Tile tile)
        {
            Tile lastTile = selectedPath[selectedPath.Count - 1];

            if (!tile.IsWalkable)
            {
                Debug.LogWarning($"Invalid Move | Tile is not walkable | X: {tile.X}, Y: {tile.Y}");
                return;
            }

            if (!AreAdjacent(lastTile, tile))
            {
                Debug.LogWarning($"Invalid Move | Tiles are not adjacent | From: X{lastTile.X}, Y{lastTile.Y} To: X{tile.X}, Y{tile.Y}");
                return;
            }

            AddTileToPath(tile);
        }

        private Tile GetTileUnderPointer()
        {
            if (mainCamera == null)
            {
                return null;
            }

            Vector3 mousePosition = UnityEngine.Input.mousePosition;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            Vector2 worldPoint = new Vector2(worldPosition.x, worldPosition.y);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider == null)
            {
                return null;
            }

            return hit.collider.GetComponentInParent<Tile>();
        }

        private void AddTileToPath(Tile tile)
        {
            if (tile == null || selectedPath.Contains(tile))
            {
                return;
            }

            selectedPath.Add(tile);
            tile.Select();
            pathChangedDuringDrag = true;
            UpdatePathLine();

            Debug.Log($"Tile Added To Path | X: {tile.X}, Y: {tile.Y}");
        }

        private void RemoveLastTileFromPath()
        {
            if (selectedPath.Count == 0)
            {
                return;
            }

            RemoveTileAt(selectedPath.Count - 1);
        }

        private void RemoveTilesAfterIndex(int pathIndex)
        {
            for (int i = selectedPath.Count - 1; i > pathIndex; i--)
            {
                RemoveTileAt(i);
            }
        }

        private void RemoveTilesFromIndex(int pathIndex)
        {
            for (int i = selectedPath.Count - 1; i >= pathIndex; i--)
            {
                RemoveTileAt(i);
            }
        }

        private void RemoveTileAt(int pathIndex)
        {
            if (pathIndex < 0 || pathIndex >= selectedPath.Count)
            {
                return;
            }

            Tile tile = selectedPath[pathIndex];

            selectedPath.RemoveAt(pathIndex);

            if (tile != null)
            {
                tile.Deselect();
                Debug.Log($"Tile Removed From Path | X: {tile.X}, Y: {tile.Y}");
            }

            pathChangedDuringDrag = true;
            UpdatePathLine();
        }

        private void UpdatePathLine()
        {
            if (pathLineRenderer == null)
            {
                return;
            }

            pathLineRenderer.positionCount = selectedPath.Count;

            for (int i = 0; i < selectedPath.Count; i++)
            {
                Vector3 tilePosition = selectedPath[i].transform.position;
                tilePosition.z = pathLineZPosition;

                pathLineRenderer.SetPosition(i, tilePosition);
            }
        }

        private bool CanTapUndoLastTile()
        {
            if (!startedWithExistingPath || pointerStartTile == null || selectedPath.Count == 0)
            {
                return false;
            }

            return selectedPath[selectedPath.Count - 1] == pointerStartTile;
        }

        private bool AreAdjacent(Tile firstTile, Tile secondTile)
        {
            if (firstTile == null || secondTile == null)
            {
                return false;
            }

            return firstTile.IsNeighbour(secondTile);
        }

        public void ConfirmPath()
        {
            if (selectedPath.Count == 0)
            {
                Debug.LogWarning("Cannot confirm path. Path is empty.");
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager is missing.");
                return;
            }

            GameManager.Instance.ChangeState(RenkYolu.Core.GameState.Walking);

            if (gameFlowUI != null)
            {
                gameFlowUI.HideConfirmButton();
            }

            if (playerMovement != null)
            {
                playerMovement.StartMovement(selectedPath);
            }
            else
            {
                Debug.LogError("PlayerMovement reference is missing on PathInputManager!");
            }

            Debug.Log($"Path Confirmed | Tile Count: {selectedPath.Count}");
        }

    }
}
