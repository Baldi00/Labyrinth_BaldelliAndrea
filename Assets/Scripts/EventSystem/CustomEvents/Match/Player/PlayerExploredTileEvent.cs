using UnityEngine;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player explores a tile
    /// </summary>
    public class PlayerExploredTileEvent : IGameEvent
    {
        public Vector2Int PositionOnGrid { set; get; }
    }
}