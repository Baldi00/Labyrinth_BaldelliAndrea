using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DBGA.Tiles
{
    /// <summary>
    /// A list of tiles element that should be placed on the map
    /// </summary>
    [CreateAssetMenu(fileName = "TilesList", menuName = "Labyrinth/Create Tiles List", order = 1)]
    public class TilesList : ScriptableObject
    {
        public List<TileListItem> availableTiles;

        /// <summary>
        /// Returns the probability weight of the given tile in this list
        /// </summary>
        /// <param name="tile">The tile you want to know the probability weight of</param>
        /// <returns>The probability weight of the given tile in this list</returns>
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
