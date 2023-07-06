using DBGA.Common;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when the player wants to shoot an arrow (doesn't mean the arrow will be actually shot in game)
    /// </summary>
    public class InputArrowShotEvent : IGameEvent
    {
        public Direction Direction { set; get; }
    }
}