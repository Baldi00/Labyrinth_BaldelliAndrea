using UnityEngine;

namespace DBGA.Tiles
{
    /// <summary>
    /// An item of tiles list containing the game object prefab of the tile along with the connected Tile component
    /// and the probability weight of the tile
    /// </summary>
    [System.Serializable]
    public class TileListItem
    {
        public GameObject tilePrefab;
        public Tile tile;
        public float probabilityWeight;
    }
}
