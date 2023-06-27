using DBGA.Common;

namespace DBGA.Tiles
{
    /// <summary>
    /// An available cross on a tile
    /// 
    /// e.g.
    /// direction1 = Up, direction2 = Down
    /// means that this tile can be crossed from Up to Down
    /// 
    /// (can be used in both monodirectional or bidirectional way)
    /// </summary>
    [System.Serializable]
    public struct Cross
    {
        public Direction direction1;
        public Direction direction2;
    }
}
