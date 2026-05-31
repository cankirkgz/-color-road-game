using UnityEngine;

namespace RenkYolu.Grid.Data
{
    [CreateAssetMenu(fileName = "TileColorData", menuName = "Renk Yolu/Tile Color Data")]
    public class TileColorData : ScriptableObject
    {
        [SerializeField] private RenkYolu.Grid.TileColorType colorType;
        [SerializeField] private Color color;

        public RenkYolu.Grid.TileColorType ColorType => colorType;
        public Color Color => color;
    }
}