namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player loses because it has no more arrow remaining
    /// (called after a wall is hit to avoid this case:
    /// Player shoots last arrow > You lose is shown > Arrow hits monster > You win is shown)
    /// </summary>
    public class PlayerLostForNoArrowRemainingEvent : IGameEvent
    {
        public int PlayerNumber { set; get; }
    }
}