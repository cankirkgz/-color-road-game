using RenkYolu.Core;
using RenkYolu.Grid;
using RenkYolu.Managers;
using UnityEngine;

namespace RenkYolu.InputSystem
{
    public class TileRevealTestInput : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.CurrentState != GameState.Drawing)
            {
                return;
            }

            if (!UnityEngine.Input.GetMouseButtonDown(1))
            {
                return;
            }

            Tile tile = GetTileUnderPointer();

            if (tile == null)
            {
                return;
            }

            tile.Reveal();

            Debug.Log($"Test Reveal Tile | X: {tile.X}, Y: {tile.Y}");
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
    }
}