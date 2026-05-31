using UnityEngine;

namespace RenkYolu.Grid.Data
{
    [CreateAssetMenu(
        fileName = "TileDefinition",
        menuName = "Renk Yolu/Tile Definition"
    )]
    public class TileDefinitionData : ScriptableObject
    {
        [SerializeField] private TileColorType colorType;
        [SerializeField] private TileOperationType operationType;

        public TileColorType ColorType => colorType;
        public TileOperationType OperationType => operationType;
    }
}