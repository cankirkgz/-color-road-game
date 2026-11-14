using UnityEngine;

namespace RenkYolu.Grid
{
    public static class TileOperationCalculator
    {
        public static int ApplyOperation(int currentScore, TileOperationType operationType, int operationValue)
        {
            return (int)ApplyOperation((long)currentScore, operationType, operationValue);
        }

        public static long ApplyOperation(long currentScore, TileOperationType operationType, int operationValue)
        {
            switch (operationType)
            {
                case TileOperationType.PlusOne:
                case TileOperationType.PlusFive:
                case TileOperationType.PlusTen:
                    return currentScore + operationValue;

                case TileOperationType.MinusFive:
                case TileOperationType.MinusTen:
                case TileOperationType.MinusFifteen:
                    return currentScore + operationValue;

                case TileOperationType.MultiplyTwo:
                case TileOperationType.MultiplyThree:
                    return currentScore * operationValue;

                case TileOperationType.DivideTwo:
                    if (operationValue == 0)
                    {
                        Debug.LogError("Divide operation failed. Operation value cannot be 0.");
                        return currentScore;
                    }

                    return currentScore / operationValue;

                default:
                    Debug.LogWarning($"Unsupported tile operation: {operationType}");
                    return currentScore;
            }
        }

        public static int DefaultValue(TileOperationType operation)
        {
            return operation switch
            {
                TileOperationType.PlusOne => 1,
                TileOperationType.PlusFive => 5,
                TileOperationType.PlusTen => 10,
                TileOperationType.MinusFive => -5,
                TileOperationType.MinusTen => -10,
                TileOperationType.MinusFifteen => -15,
                TileOperationType.MultiplyTwo => 2,
                TileOperationType.MultiplyThree => 3,
                TileOperationType.DivideTwo => 2,
                _ => 0
            };
        }
    }
}
