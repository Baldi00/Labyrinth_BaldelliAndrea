using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DBGA.Common;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private List<Cross> availableCrossings;

        private Vector2Int positionOnGrid;
        private bool hasMonser;
        private bool hasWell;
        private bool hasTeleport;
        private bool playerExplored;


        /// <summary>
        /// Sets the position of the tile on the grid
        /// </summary>
        /// <param name="positionOnGrid">The position on the grid</param>
        public void SetPositionOnGrid(Vector2Int positionOnGrid)
        {
            this.positionOnGrid = positionOnGrid;
        }

        /// <summary>
        /// Returns the list of available bidirectional crossings that can be done on this tile
        /// </summary>
        /// <returns>The list of available bidirectional crossings that can be done on this tile</returns>
        public List<Cross> GetAvailableCrossings()
        {
            return new List<Cross>(availableCrossings);
        }

        /// <summary>
        /// Returns the list of available directions from this tile
        /// </summary>
        /// <returns>The list of available directions from this tile</returns>
        public List<Direction> GetAvailableDirections()
        {
            HashSet<Direction> availableDirections = new HashSet<Direction>();
            foreach (Cross cross in availableCrossings)
            {
                availableDirections.Add(cross.direction1);
                availableDirections.Add(cross.direction2);
            }
            return availableDirections.ToList<Direction>();
        }
    }
}
