namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when an arrow is actually shot in game
    /// </summary>
    public class ArrowShotEvent : GameEvent
    {
        public int remainingArrows;
    }
}