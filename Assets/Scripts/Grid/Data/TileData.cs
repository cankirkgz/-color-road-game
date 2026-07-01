namespace RenkYolu.Grid.Data
{
    [System.Serializable]
    public class TileData
    {
        public int tileId;
        public int x;
        public int y;
        public TileColorType colorType;
        public TileOperationType operationType;
        public int operationValue;
        public bool isVisible;
        public bool isWalkable;
        public TileSpecialType specialType;

        public TileData(
            int newTileId,
            int gridX,
            int gridY,
            TileColorType newColorType,
            TileOperationType newOperationType,
            int newOperationValue,
            bool visible,
            bool walkable,
            TileSpecialType newSpecialType
        )
        {
            tileId = newTileId;
            x = gridX;
            y = gridY;
            colorType = newColorType;
            operationType = newOperationType;
            operationValue = newOperationValue;
            isVisible = visible;
            isWalkable = walkable;
            specialType = newSpecialType;
        }
    }
}