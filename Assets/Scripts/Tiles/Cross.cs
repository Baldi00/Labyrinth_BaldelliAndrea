using UnityEngine;
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
        [SerializeField]
        private Direction direction1;
        [SerializeField]
        private Direction direction2;

        public Direction Direction1 { set => direction1 = value; get => direction1; }
        public Direction Direction2 { set => direction2 = value; get => direction2; }
    }
}
