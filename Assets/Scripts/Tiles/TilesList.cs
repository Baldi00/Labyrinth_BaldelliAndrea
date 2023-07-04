using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DBGA.Tiles
{
    [CreateAssetMenu(fileName = "TilesList", menuName = "Labyrinth/Create Tiles List", order = 1)]
    public class TilesList : ScriptableObject
    {
        public List<TileListItem> availableTiles;

        public float GetProbabilityWeight(Tile tile)
        {
            if (availableTiles.Any<TileListItem>(item => item.tile == tile))
                return availableTiles
                    .Where<TileListItem>(item => item.tile == tile)
                    .ElementAt<TileListItem>(0).probabilityWeight;

            return 0;
        }
    }
}
