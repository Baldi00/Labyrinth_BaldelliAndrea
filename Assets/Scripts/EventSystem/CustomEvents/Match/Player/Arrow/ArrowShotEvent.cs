namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when an arrow is actually shot in game
    /// </summary>
    public class ArrowShotEvent : IGameEvent
    {
        public int RemainingArrows { set; get; }
    }
}