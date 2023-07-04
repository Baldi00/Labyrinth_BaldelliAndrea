using DBGA.Common;
using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private List<Direction> outDirections;

        public bool HasMonster { get; set; }
        public bool HasWell { get; set; }
        public bool HasTeleport { get; set; }
        public bool PlayerExplored { set; get; }
        public List<Direction> OutDirections { get => outDirections; }
        public Vector2Int PositionOnGrid { set; get; }
    }
}
