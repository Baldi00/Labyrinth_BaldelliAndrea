using UnityEngine;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player starts its turn
    /// </summary>
    public class PlayerStartedTurnEvent : GameEvent
    {
        public int playerNumber;
        public int playerArrowsCount;
        public Color playerColor;
    }
}