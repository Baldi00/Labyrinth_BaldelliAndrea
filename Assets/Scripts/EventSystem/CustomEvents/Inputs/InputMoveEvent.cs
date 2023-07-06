using DBGA.Common;

namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when the player wants to move (doesn't mean the player will actually move in game)
    /// </summary>
    public class InputMoveEvent : IGameEvent
    {
        public Direction Direction { set; get; }
    }
}