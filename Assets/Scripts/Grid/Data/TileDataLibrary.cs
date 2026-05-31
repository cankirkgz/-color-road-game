using RenkYolu.Grid;
using UnityEngine;

namespace RenkYolu.Grid.Data
{
    public static class TileDataLibrary
    {
        public static TileColorData GetColorData(TileColorType colorType)
        {
            TileColorData[] colorDataList = Resources.LoadAll<TileColorData>("TileColors");

            foreach (TileColorData colorData in colorDataList)
            {
                if (colorData.ColorType == colorType)
                {
                    return colorData;
                }
            }

            Debug.LogError($"Tile color data not found: {colorType}");
            return null;
        }

        public static TileOperationData GetOperationData(TileOperationType operationType)
        {
            TileOperationData[] operationDataList = Resources.LoadAll<TileOperationData>("TileOperations");

            foreach (TileOperationData operationData in operationDataList)
            {
                if (operationData.OperationType == operationType)
                {
                    return operationData;
                }
            }

            Debug.LogError($"Tile operation data not found: {operationType}");
            return null;
        }
    }
}