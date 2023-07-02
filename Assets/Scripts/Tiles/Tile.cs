using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DBGA.Common;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class Tile : MonoBehaviour
    {
        public bool HasMonser { set; get; }
        public bool HasWell { set; get; }
        public bool HasTeleport { set; get; }
        public bool PlayerExplored { set; get; }

        public Vector2Int PositionOnGrid { set; get; }
    }
}
