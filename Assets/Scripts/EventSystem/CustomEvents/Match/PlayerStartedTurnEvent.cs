using UnityEngine;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player starts its turn
    /// </summary>
    public class PlayerStartedTurnEvent : IGameEvent
    {
        public int PlayerNumber { set; get; }
        public int PlayerArrowsCount { set; get; }
        public Color PlayerColor { set; get; }
    }
}