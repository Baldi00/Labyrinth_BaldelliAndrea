using UnityEngine;

namespace DBGA.EventSystem
{
    public class PlayerExploredTileEvent : GameEvent
    {
        public Vector2Int positionOnGrid;
    }
}