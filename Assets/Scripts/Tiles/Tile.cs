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
        private Cross[] availableCrossings;

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

        /// <summary>
        /// Remove all crossings that can go to or from the given direction
        /// </summary>
        /// <param name="direction">The direction to remove from available crossings</param>
        public void RemoveCrossWithDirection(Direction direction)
        {
            List<Cross> availableCrossingsList = availableCrossings.ToList<Cross>();
            List<Cross> toRemove = new List<Cross>();

            foreach(Cross cross in availableCrossingsList)
                if (cross.direction1 == direction || cross.direction2 == direction)
                    toRemove.Add(cross);

            foreach (Cross cross in toRemove)
                availableCrossingsList.Remove(cross);

            availableCrossings = availableCrossingsList.ToArray<Cross>();
        }

        /// <summary>
        /// Checks if there is a crossing that goes from or to the given direction
        /// </summary>
        /// <param name="direction">The direction to test</param>
        /// <returns>True if there is a crossing that goes from or to the given direction, false otherwise</returns>
        public bool CrossingsContainDirection(Direction direction)
        {
            foreach (Cross cross in availableCrossings)
                if (cross.direction1 == direction || cross.direction2 == direction)
                    return true;

            return false;
        }
    }
}
