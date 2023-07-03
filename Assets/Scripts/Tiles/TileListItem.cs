using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Tiles
{
    [System.Serializable]
    public struct TileListItem
    {
        public GameObject tilePrefab;
        public Tile tile;
        public float probabilityWeight;
    }
}
