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
        [SerializeField]
        private GameObject tilePrefab;
        [SerializeField]
        private Tile tile;
        [SerializeField]
        private float probabilityWeight;

        public GameObject TilePrefab { set => tilePrefab = value; get => tilePrefab; }
        public Tile Tile { set => tile = value; get => tile; }
        public float ProbabilityWeight { set => probabilityWeight = value; get => probabilityWeight; }
    }
}
