using System;
using System.Collections;
using System.Collections.Generic;
using RenkYolu.Grid;
using UnityEngine;
using DG.Tweening;
using RenkYolu.Managers;
using RenkYolu.UI;

namespace RenkYolu.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private float zPosition = 0f;

        [Header("Animation Settings")]
        [SerializeField] private float squashAmount = 0.15f;
        [SerializeField] private float stretchAmount = 0.12f;

        public event Action<Tile> OnTileReached;
        public event Action OnMovementFinished;
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

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }
            else
            {
                Debug.LogWarning("ScoreManager is missing. Score will not be reset.");
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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(RenkYolu.Core.GameState.LevelComplete);
            }
            else
            {
                Debug.LogError("GameManager is missing. Cannot change state after movement.");
            }

            OnMovementFinished?.Invoke();

            Debug.Log("Player movement finished.");
        }

        private IEnumerator MoveToTile(Tile tile)
        {
            Vector3 targetPosition = tile.transform.position;
            targetPosition.z = zPosition;

            Vector3 originalScale = transform.localScale;

            Vector3 squashScale = new Vector3(
                originalScale.x + stretchAmount,
                originalScale.y - squashAmount,
                originalScale.z
            );

            Vector3 stretchScale = new Vector3(
                originalScale.x - squashAmount,
                originalScale.y + stretchAmount,
                originalScale.z
            );

            Sequence movementSequence = DOTween.Sequence();

            movementSequence.Append(
                transform.DOScale(squashScale, moveDuration * 0.2f)
                    .SetEase(Ease.OutQuad)
            );

            movementSequence.Append(
                transform.DOMove(targetPosition, moveDuration)
                    .SetEase(Ease.OutQuad)
            );

            movementSequence.Join(
                transform.DOScale(stretchScale, moveDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
            );

            movementSequence.Append(
                transform.DOScale(originalScale, moveDuration * 0.25f)
                    .SetEase(Ease.OutBack)
            );

            yield return movementSequence.WaitForCompletion();

            transform.position = targetPosition;
            transform.localScale = originalScale;

            tile.Deselect();
            tile.Reveal();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ApplyOperationFromTile(tile);

                int comboValue = ScoreManager.Instance.UpdateComboFromTile(tile);

                if (comboValue >= ScoreManager.Instance.MinimumComboToShow &&
                    FloatingTextSpawner.Instance != null)
                {
                    FloatingTextSpawner.Instance.ShowCombo(tile, comboValue);
                }

                if (comboValue >= 3 && GameplayFXManager.Instance != null)
                {
                    GameplayFXManager.Instance.PlayComboFx(tile, transform);
                }

                bool bonusApplied = ScoreManager.Instance.TryApplyColorBonus(tile);

                if (bonusApplied && FloatingTextSpawner.Instance != null)
                {
                    FloatingTextSpawner.Instance.ShowColorBonus(tile, 2);
                }

                if (bonusApplied && GameplayFXManager.Instance != null)
                {
                    GameplayFXManager.Instance.PlayBonusFx(tile, transform);
                }

                if (IsCriticalOperation(tile.OperationType) && GameplayFXManager.Instance != null)
                {
                    GameplayFXManager.Instance.PlayCriticalFx(tile, transform);
                }
            }
            else
            {
                Debug.LogWarning("ScoreManager is missing. Tile operation cannot be applied.");
            }

            if (FloatingTextSpawner.Instance != null)
            {
                FloatingTextSpawner.Instance.ShowTileOperation(tile);
            }
            else
            {
                Debug.LogWarning("FloatingTextSpawner is missing. Floating text cannot be shown.");
            }

            OnTileReached?.Invoke(tile);

            Debug.Log($"Player moved to tile | X: {tile.X}, Y: {tile.Y} | Current Score: {ScoreManager.Instance?.CurrentScore}");
        }

        private bool IsCriticalOperation(TileOperationType operationType)
        {
            return operationType == TileOperationType.MultiplyTwo ||
                operationType == TileOperationType.MultiplyThree;
        }
    }
}