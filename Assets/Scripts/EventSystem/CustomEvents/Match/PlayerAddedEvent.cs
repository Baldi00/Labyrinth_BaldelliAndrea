using UnityEngine;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player is added into the game
    /// </summary>
    public class PlayerAddedEvent : IGameEvent
    {
        public int PlayerNumber { set; get; }
    }
}