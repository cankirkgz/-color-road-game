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

        [Header("Spike Push Settings")]
        [SerializeField] private int spikeShortcutBonus = 50;
        [SerializeField] private float spikeFallDistance = 1f;

        [Header("Animation Settings")]
        [SerializeField] private float squashAmount = 0.15f;
        [SerializeField] private float stretchAmount = 0.12f;

        [Header("Hit Feedback")]
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        [SerializeField] private Color hitFeedbackColor = new Color(1f, 0.12f, 0.12f);
        [SerializeField] private float hitFeedbackDuration = 0.08f;
        [SerializeField] private float hitPunchScale = 0.12f;

        private Color playerOriginalColor = Color.white;
        private Vector3 playerOriginalScale;

        public event Action<Tile> OnTileReached;
        public event Action OnMovementFinished;
        private bool isMoving;

        public bool IsMoving => isMoving;

        private void Awake()
        {
            playerOriginalScale = transform.localScale;

            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (playerSpriteRenderer != null)
            {
                playerOriginalColor = playerSpriteRenderer.color;
            }
        }

        public bool StartMovement(IReadOnlyList<Tile> path)
        {
            if (isMoving)
            {
                Debug.LogWarning("Movement cannot start. Player is already moving.");
                return false;
            }

            if (!ValidatePathBeforeMovement(path))
            {
                return false;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetCombo();
            }
            else
            {
                Debug.LogWarning("ScoreManager is missing. Combo will not be reset.");
            }

            StartCoroutine(MoveThroughPath(path));
            return true;
        }

        private IEnumerator MoveThroughPath(IReadOnlyList<Tile> path)
        {
            isMoving = true;

            Tile actualFinishTile = null;

            for (int i = 0; i < path.Count; i++)
            {
                Tile targetTile = path[i];

                if (!IsTileValidForMovement(targetTile, i))
                {
                    StopMovementBecauseOfInvalidPath();
                    yield break;
                }

                yield return MoveToTile(targetTile);
                actualFinishTile = targetTile;

                if (targetTile.IsSpike)
                {
                    if (!TryGetSpikePushTarget(targetTile, out Tile pushTarget))
                    {
                        Vector3 fallPosition = GetSpikeFallPosition(targetTile);

                        Debug.LogWarning(
                            $"Spike pushed player outside grid | " +
                            $"Spike X:{targetTile.X}, Y:{targetTile.Y}, Direction:{targetTile.SpikeDirection}"
                        );

                        yield return MoveToPositionOnly(fallPosition);

                        StopMovementBecauseOfSpikeFail(
                            "Spike pushed player outside the grid.",
                            targetTile
                        );
                        yield break;
                    }

                    if (!IsTileValidForMovement(pushTarget, i + 1))
                    {
                        yield return MoveToPositionOnly(pushTarget.transform.position);

                        StopMovementBecauseOfSpikeFail(
                            "Spike pushed player into blocked tile.",
                            targetTile
                        );
                        yield break;
                    }

                    int pushTargetIndex = FindTileIndexInPath(path, pushTarget, i + 1);

                    if (pushTargetIndex < 0)
                    {
                        bool spikeIsLastDrawnTile = i == path.Count - 1;

                        if (spikeIsLastDrawnTile)
                        {
                            Debug.Log(
                                $"Spike Final Push | " +
                                $"Spike X:{targetTile.X}, Y:{targetTile.Y}, Direction:{targetTile.SpikeDirection} | " +
                                $"Final Landing X:{pushTarget.X}, Y:{pushTarget.Y}"
                            );

                            yield return MoveToTile(pushTarget);
                            actualFinishTile = pushTarget;

                            break;
                        }

                        Debug.LogWarning(
                            $"Spike pushed player to a tile that is not in the drawn path | " +
                            $"Spike X:{targetTile.X}, Y:{targetTile.Y}, Direction:{targetTile.SpikeDirection} | " +
                            $"Push Target X:{pushTarget.X}, Y:{pushTarget.Y}"
                        );

                        yield return MoveToPositionOnly(pushTarget.transform.position);

                        StopMovementBecauseOfSpikeFail(
                            "Spike target tile was not included in the drawn path.",
                            targetTile
                        );
                        yield break;
                    }

                    int skippedStartIndex = i + 1;
                    int skippedEndIndex = pushTargetIndex - 1;
                    int skippedTileCount = skippedEndIndex - skippedStartIndex + 1;

                    if (skippedTileCount > 0)
                    {
                        Debug.Log(
                            $"Spike Shortcut Detected | " +
                            $"Spike Index:{i} | " +
                            $"Push Target Index:{pushTargetIndex} | " +
                            $"Skipped Start:{skippedStartIndex} | " +
                            $"Skipped End:{skippedEndIndex} | " +
                            $"Skipped Count:{skippedTileCount}"
                        );

                        ApplySkippedPathScore(path, skippedStartIndex, skippedEndIndex);

                        if (ScoreManager.Instance != null)
                        {
                            int scoreBeforeBonus = ScoreManager.Instance.CurrentScore;

                            ScoreManager.Instance.AddShortcutBonus(
                                spikeShortcutBonus,
                                $"Spike shortcut skipped {skippedTileCount} tile(s)"
                            );

                            Debug.Log(
                                $"Spike Shortcut Bonus Confirmed | " +
                                $"Bonus:+{spikeShortcutBonus} | " +
                                $"Score:{scoreBeforeBonus} -> {ScoreManager.Instance.CurrentScore}"
                            );
                        }
                        else
                        {
                            Debug.LogWarning("ScoreManager is missing. Shortcut bonus cannot be added.");
                        }

                        Debug.Log(
                            $"Spike Shortcut Used | " +
                            $"Skipped Tiles:{skippedTileCount} | " +
                            $"Bonus:+{spikeShortcutBonus}"
                        );
                    }
                    else
                    {
                        Debug.Log(
                            $"Spike Push Without Shortcut Bonus | " +
                            $"Push target is the next drawn tile. " +
                            $"Spike Index:{i} | Push Target Index:{pushTargetIndex}"
                        );
                    }

                    Debug.Log(
                        $"Spike Push Started | " +
                        $"Spike X:{targetTile.X}, Y:{targetTile.Y}, Direction:{targetTile.SpikeDirection} | " +
                        $"Push Target X:{pushTarget.X}, Y:{pushTarget.Y} | " +
                        $"Push Target Path Index:{pushTargetIndex}"
                    );

                    yield return MoveToTile(pushTarget);
                    actualFinishTile = pushTarget;

                    i = pushTargetIndex;
                }
            }

            isMoving = false;

            Tile finishTile = actualFinishTile != null
                ? actualFinishTile
                : GetLastValidTile(path);

            if (GameResultManager.Instance != null)
            {
                GameResultManager.Instance.EvaluateResult(finishTile);
            }
            else
            {
                Debug.LogError("GameResultManager is missing. Cannot evaluate result after movement.");
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

            if (tile.IsSpike)
            {
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.ResetComboBecauseOfSpike(tile);
                }

                tile.PlaySpikeEffect();
                PlayHitFeedback();

                Debug.Log(
                    $"Player reached spike tile | " +
                    $"X:{tile.X}, Y:{tile.Y}, Direction:{tile.SpikeDirection}. " +
                    $"Score, combo, color bonus and floating operation skipped."
                );

                OnTileReached?.Invoke(tile);

                yield break;
            }

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

                /*if (bonusApplied && GameplayFXManager.Instance != null)
                {
                    GameplayFXManager.Instance.PlayBonusFx(tile, transform);
                }*/

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

        private IEnumerator MoveToPositionOnly(Vector3 targetPosition)
        {
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
        }

        private void PlayHitFeedback()
        {
            transform.DOKill();

            Vector3 originalScale = playerOriginalScale;

            if (originalScale == Vector3.zero)
            {
                originalScale = transform.localScale;
            }

            Sequence hitSequence = DOTween.Sequence();

            hitSequence.Append(
                transform.DOScale(originalScale * (1f + hitPunchScale), hitFeedbackDuration)
                    .SetEase(Ease.OutQuad)
            );
            hitSequence.Append(
                transform.DOScale(originalScale, hitFeedbackDuration)
                    .SetEase(Ease.InQuad)
            );

            if (playerSpriteRenderer != null)
            {
                playerSpriteRenderer.DOKill();

                Color baseColor = playerOriginalColor;
                Sequence colorSequence = DOTween.Sequence();

                colorSequence.Append(
                    playerSpriteRenderer.DOColor(hitFeedbackColor, hitFeedbackDuration)
                        .SetEase(Ease.OutQuad)
                );
                colorSequence.Append(
                    playerSpriteRenderer.DOColor(baseColor, hitFeedbackDuration)
                        .SetEase(Ease.InQuad)
                );
                colorSequence.OnComplete(() => playerSpriteRenderer.color = baseColor);
            }

            Debug.Log("Player hit feedback played.");
        }

        private bool ValidatePathBeforeMovement(IReadOnlyList<Tile> path)
        {
            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("Movement cannot start. Path is empty.");
                return false;
            }

            for (int i = 0; i < path.Count; i++)
            {
                if (!IsTileValidForMovement(path[i], i))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsTileValidForMovement(Tile tile, int pathIndex)
        {
            if (tile == null)
            {
                Debug.LogWarning($"Movement blocked: path contains null tile at index {pathIndex}.");
                return false;
            }

            if (tile.IsWall)
            {
                Debug.LogWarning(
                    $"Movement blocked: path contains wall tile at index {pathIndex} | " +
                    $"X:{tile.X}, Y:{tile.Y}"
                );

                tile.PlayBlockedFeedback();
                return false;
            }

            if (!tile.IsWalkable)
            {
                Debug.LogWarning(
                    $"Movement blocked: path contains non-walkable tile at index {pathIndex} | " +
                    $"X:{tile.X}, Y:{tile.Y}, SpecialType:{tile.SpecialType}"
                );

                tile.PlayBlockedFeedback();
                return false;
            }

            return true;
        }

        private bool TryGetSpikePushTarget(Tile spikeTile, out Tile pushTarget)
        {
            pushTarget = null;

            if (spikeTile == null || !spikeTile.IsSpike)
            {
                return false;
            }

            switch (spikeTile.SpikeDirection)
            {
                case GridDirection.Up:
                    pushTarget = spikeTile.UpNeighbour;
                    break;

                case GridDirection.Down:
                    pushTarget = spikeTile.DownNeighbour;
                    break;

                case GridDirection.Left:
                    pushTarget = spikeTile.LeftNeighbour;
                    break;

                case GridDirection.Right:
                    pushTarget = spikeTile.RightNeighbour;
                    break;

                default:
                    Debug.LogWarning(
                        $"Spike push failed. Direction is None | " +
                        $"X:{spikeTile.X}, Y:{spikeTile.Y}"
                    );
                    return false;
            }

            if (pushTarget == null)
            {
                Debug.LogWarning(
                    $"Spike push failed. Target tile is outside grid | " +
                    $"Spike X:{spikeTile.X}, Y:{spikeTile.Y}, Direction:{spikeTile.SpikeDirection}"
                );
                return false;
            }

            return true;
        }

        private int FindTileIndexInPath(IReadOnlyList<Tile> path, Tile targetTile, int startIndex)
        {
            if (path == null || targetTile == null)
            {
                return -1;
            }

            for (int i = startIndex; i < path.Count; i++)
            {
                if (path[i] == targetTile)
                {
                    return i;
                }
            }

            return -1;
        }

        private void ApplySkippedPathScore(IReadOnlyList<Tile> path, int startIndex, int endIndex)
        {
            if (path == null)
            {
                return;
            }

            if (ScoreManager.Instance == null)
            {
                Debug.LogWarning("ScoreManager is missing. Skipped path score cannot be applied.");
                return;
            }

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i < 0 || i >= path.Count)
                {
                    continue;
                }

                Tile skippedTile = path[i];

                if (skippedTile == null)
                {
                    continue;
                }

                if (skippedTile.IsWall)
                {
                    Debug.LogWarning(
                        $"Skipped score ignored for wall tile | " +
                        $"Index:{i}, X:{skippedTile.X}, Y:{skippedTile.Y}"
                    );

                    continue;
                }

                if (skippedTile.IsSpike)
                {
                    Debug.LogWarning(
                        $"Skipped score ignored for another spike tile | " +
                        $"Index:{i}, X:{skippedTile.X}, Y:{skippedTile.Y}"
                    );

                    skippedTile.Deselect();
                    skippedTile.Reveal();

                    continue;
                }

                skippedTile.Deselect();
                skippedTile.Reveal();

                ScoreManager.Instance.ApplyOperationFromTile(skippedTile);

                int comboValue = ScoreManager.Instance.UpdateComboFromTile(skippedTile);

                if (comboValue >= ScoreManager.Instance.MinimumComboToShow &&
                    FloatingTextSpawner.Instance != null)
                {
                    FloatingTextSpawner.Instance.ShowCombo(skippedTile, comboValue);
                }

                bool bonusApplied = ScoreManager.Instance.TryApplyColorBonus(skippedTile);

                if (bonusApplied && FloatingTextSpawner.Instance != null)
                {
                    FloatingTextSpawner.Instance.ShowColorBonus(skippedTile, 2);
                }

                if (FloatingTextSpawner.Instance != null)
                {
                    FloatingTextSpawner.Instance.ShowTileOperation(skippedTile);
                }

                Debug.Log(
                    $"Skipped Tile Score Applied By Spike Shortcut | " +
                    $"Index:{i}, X:{skippedTile.X}, Y:{skippedTile.Y}"
                );
            }
        }

        private Vector3 GetSpikeFallPosition(Tile spikeTile)
        {
            Vector3 direction = GetSpikeDirectionVector(spikeTile);
            Vector3 fallPosition = spikeTile.transform.position + direction * spikeFallDistance;
            fallPosition.z = zPosition;

            return fallPosition;
        }

        private Vector3 GetSpikeDirectionVector(Tile spikeTile)
        {
            if (spikeTile == null)
            {
                return Vector3.zero;
            }

            switch (spikeTile.SpikeDirection)
            {
                case GridDirection.Up:
                    return Vector3.up;

                case GridDirection.Down:
                    return Vector3.down;

                case GridDirection.Left:
                    return Vector3.left;

                case GridDirection.Right:
                    return Vector3.right;

                default:
                    return Vector3.zero;
            }
        }

        private void StopMovementBecauseOfSpikeFail(string reason, Tile spikeTile)
        {
            isMoving = false;

            transform.DOKill();

            if (spikeTile != null)
            {
                spikeTile.PlaySpikeEffect();
            }

            PlayHitFeedback();

            if (GameResultManager.Instance != null)
            {
                GameResultManager.Instance.ForceFailBySpike(reason, spikeTile);
            }
            else
            {
                Debug.LogError("GameResultManager is missing. Cannot fail by spike.");
            }

            OnMovementFinished?.Invoke();

            Debug.LogWarning($"Player movement stopped because of spike trap | Reason: {reason}");
        }

        private void StopMovementBecauseOfInvalidPath()
        {
            isMoving = false;

            transform.DOKill();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(RenkYolu.Core.GameState.Drawing);
            }
            else
            {
                Debug.LogWarning("GameManager is missing. Cannot return to Drawing state after invalid movement path.");
            }

            OnMovementFinished?.Invoke();

            Debug.LogWarning("Player movement stopped because path became invalid.");
        }

        private Tile GetLastValidTile(IReadOnlyList<Tile> path)
        {
            if (path == null || path.Count == 0)
            {
                return null;
            }

            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (path[i] != null)
                {
                    return path[i];
                }
            }

            return null;
        }

        private bool IsCriticalOperation(TileOperationType operationType)
        {
            return operationType == TileOperationType.MultiplyTwo ||
                operationType == TileOperationType.MultiplyThree;
        }

    }
}
