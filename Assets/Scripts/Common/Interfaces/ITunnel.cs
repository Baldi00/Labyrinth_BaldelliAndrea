using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Common
{
    public interface ITunnel
    {
        public bool CanCross(Direction enterDirection);
        public Vector2Int GetFinalDestination(Direction enterDirection);
        public List<Vector2Int> GetAllCrossingPoints(Direction enterDirection);
        public void RevealEntireTunnel(Direction enterDirection);
        public List<Direction> GetAvailableDirections();
        public Direction GetOutDirection(Direction enterDirection);
    }
}
