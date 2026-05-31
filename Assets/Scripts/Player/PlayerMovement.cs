using System.Collections;
using System.Collections.Generic;
using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private float zPosition = 0f;

        private bool isMoving;

        public bool IsMoving => isMoving;

        public void StartMovement(IReadOnlyList<Tile> path)
        {
            if (isMoving)
            {
                return;
            }

            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("Movement cannot start. Path is empty.");
                return;
            }

            StartCoroutine(MoveThroughPath(path));
        }

        private IEnumerator MoveThroughPath(IReadOnlyList<Tile> path)
        {
            isMoving = true;

            for (int i = 0; i < path.Count; i++)
            {
                Tile targetTile = path[i];

                if (targetTile == null)
                {
                    continue;
                }

                yield return MoveToTile(targetTile);
            }

            isMoving = false;

            Debug.Log("Player movement finished.");
        }

        private IEnumerator MoveToTile(Tile tile)
        {
            Vector3 startPosition = transform.position;

            Vector3 targetPosition = tile.transform.position;
            targetPosition.z = zPosition;

            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                elapsedTime += Time.deltaTime;

                float progress = elapsedTime / moveDuration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

                yield return null;
            }

            transform.position = targetPosition;

            Debug.Log($"Player moved to tile | X: {tile.X}, Y: {tile.Y}");
        }
    }
}