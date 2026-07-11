using System.Collections.Generic;
using RenkYolu.Grid;
using UnityEngine;
using UnityEngine.Serialization;

namespace RenkYolu.Levels
{
    [CreateAssetMenu(
        fileName = "LevelData",
        menuName = "Renk Yolu/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int levelId = 1;
        [SerializeField] private string levelName = "Level 1";

        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 4;
        [SerializeField] private int gridHeight = 4;

        [Header("Start Tile")]
        [SerializeField] private int startTileX = 0;
        [SerializeField] private int startTileY = 0;

        [Header("Tiles")]
        [SerializeField] private List<LevelTileData> tiles = new List<LevelTileData>();

        public int LevelId => levelId;
        public string LevelName => levelName;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public int StartTileX => startTileX;
        public int StartTileY => startTileY;
        public IReadOnlyList<LevelTileData> Tiles => tiles;

        public int ExpectedTileCount => gridWidth * gridHeight;

        public bool HasValidSize()
        {
            return gridWidth > 0 && gridHeight > 0;
        }

        public bool HasCorrectTileCount()
        {
            return tiles != null && tiles.Count == ExpectedTileCount;
        }

        public LevelTileData GetTileDataAt(int x, int y)
        {
            if (tiles == null)
            {
                return null;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                LevelTileData tile = tiles[i];

                if (tile == null)
                {
                    continue;
                }

                if (tile.X == x && tile.Y == y)
                {
                    return tile;
                }
            }

            return null;
        }

        public bool IsStartTileInsideGrid()
        {
            return startTileX >= 0 &&
                   startTileX < gridWidth &&
                   startTileY >= 0 &&
                   startTileY < gridHeight;
        }

        private void OnValidate()
        {
            if (gridWidth < 1)
            {
                gridWidth = 1;
            }

            if (gridHeight < 1)
            {
                gridHeight = 1;
            }

            if (levelId < 1)
            {
                levelId = 1;
            }

            startTileX = Mathf.Clamp(startTileX, 0, gridWidth - 1);
            startTileY = Mathf.Clamp(startTileY, 0, gridHeight - 1);

            UpdateOperationValues();
            ValidateSpecialTiles();
        }

        private void ValidateSpecialTiles()
        {
            LevelTileData startTile = GetTileDataAt(startTileX, startTileY);

            if (startTile != null &&
                (!startTile.IsWalkable ||
                 startTile.SpecialType == TileSpecialType.Wall ||
                 startTile.SpecialType == TileSpecialType.Launcher ||
                 startTile.SpecialType == TileSpecialType.Spike))
            {
                Debug.LogWarning(
                    $"Level {levelId}: Start tile cannot be Wall, Launcher, Spike or non-walkable | " +
                    $"X:{startTileX}, Y:{startTileY}, SpecialType:{startTile.SpecialType}",
                    this
                );
            }

            if (tiles == null)
            {
                return;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                LevelTileData tile = tiles[i];

                if (tile != null &&
                    tile.SpecialType == TileSpecialType.Launcher &&
                    tile.LauncherDirection == GridDirection.None)
                {
                    Debug.LogWarning(
                        $"Level {levelId}: Launcher direction cannot be None | X:{tile.X}, Y:{tile.Y}",
                        this
                    );
                }
            }
        }

        private void UpdateOperationValues()
        {
            if (tiles == null)
            {
                return;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] == null)
                {
                    continue;
                }

                tiles[i].RefreshOperationValue();
            }
        }
    }

    [System.Serializable]
    public class LevelTileData
    {
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private TileColorType colorType = TileColorType.Red;
        [SerializeField] private TileOperationType operationType = TileOperationType.PlusOne;
        [SerializeField] private int operationValue = 1;
        [SerializeField] private bool isWalkable = true;
        [SerializeField] private TileSpecialType specialType = TileSpecialType.None;
        [SerializeField]
        [FormerlySerializedAs("spikeDirection")]
        private GridDirection launcherDirection = GridDirection.None;

        public int X => x;
        public int Y => y;
        public TileColorType ColorType => colorType;
        public TileOperationType OperationType => operationType;
        public int OperationValue => operationValue;
        public bool IsWalkable => isWalkable;
        public TileSpecialType SpecialType => specialType;
        public GridDirection LauncherDirection => launcherDirection;

        public void RefreshOperationValue()
        {
            operationValue = GetDefaultOperationValue(operationType);
        }

        private int GetDefaultOperationValue(TileOperationType type)
        {
            switch (type)
            {
                case TileOperationType.PlusOne:
                    return 1;

                case TileOperationType.PlusFive:
                    return 5;

                case TileOperationType.PlusTen:
                    return 10;

                case TileOperationType.MinusFive:
                    return -5;

                case TileOperationType.MinusTen:
                    return -10;

                case TileOperationType.MinusFifteen:
                    return -15;

                case TileOperationType.MultiplyTwo:
                    return 2;

                case TileOperationType.MultiplyThree:
                    return 3;

                case TileOperationType.DivideTwo:
                    return 2;

                default:
                    return 0;
            }
        }
    }
}
