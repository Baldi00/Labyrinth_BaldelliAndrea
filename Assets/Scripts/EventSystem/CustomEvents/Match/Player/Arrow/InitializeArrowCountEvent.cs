namespace DBGA.EventSystem
{
    /// <summary>
    /// Called at the beginning of the game to initialize the arrow count UI
    /// </summary>
    public class InitializeArrowCountEvent : IGameEvent
    {
        public int RemainingArrows { set; get; }
    }
}