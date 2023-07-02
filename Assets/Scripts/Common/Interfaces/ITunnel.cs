using System.Collections.Generic;
using UnityEngine;

namespace DBGA.Common
{
    public interface ITunnel
    {
        public Vector2Int GetFinalDestination(Direction enterDirection);
        public List<Vector2Int> GetAllCrossingPoints(Direction enterDirection);
        public bool CanCross(Direction enterDirection);
        public void RevealEntireTunnel(Direction enterDirection);
    }
}
