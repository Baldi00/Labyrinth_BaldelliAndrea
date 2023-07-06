using UnityEngine;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player explores a tile
    /// </summary>
    public class PlayerExploredTileEvent : GameEvent
    {
        public Vector2Int positionOnGrid;
    }
}