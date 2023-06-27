using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DBGA.Tiles
{
    [DisallowMultipleComponent]
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private List<Cross> availableCrossings;

        private int positionX;
        private int positionY;
        private bool hasMonser;
        private bool hasWell;
        private bool hasTeleport;
        private bool playerExplored;

        public void SetPosition(int positionX, int positionY)
        {
            this.positionX = positionX;
            this.positionY = positionY;
        }

        public List<Cross> GetAvailableCrossings()
        {
            return new List<Cross>(availableCrossings);
        }

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
