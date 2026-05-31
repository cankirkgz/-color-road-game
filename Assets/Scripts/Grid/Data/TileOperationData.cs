using UnityEngine;

namespace RenkYolu.Grid.Data
{
    [CreateAssetMenu(fileName = "TileOperationData", menuName = "Renk Yolu/Tile Operation Data")]
    public class TileOperationData : ScriptableObject
    {
        [SerializeField] private TileOperationType operationType;
        [SerializeField] private string label;
        [SerializeField] private int value;

        public TileOperationType OperationType => operationType;
        public string Label => label;
        public int Value => value;
    }
}