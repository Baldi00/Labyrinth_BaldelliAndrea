using UnityEngine;

namespace DBGA.Tiles
{
    [System.Serializable]
    public class TileListItem
    {
        public GameObject tilePrefab;
        public Tile tile;
        public float probabilityWeight;
    }
}
