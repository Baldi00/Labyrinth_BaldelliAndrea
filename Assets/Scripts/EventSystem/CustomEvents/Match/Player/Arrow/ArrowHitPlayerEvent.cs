namespace DBGA.EventSystem
{
    // Called when an arrow collides with a player
    public class ArrowHitPlayerEvent : IGameEvent
    {
        public int ShooterPlayerNumber { set; get; }
        public int HitPlayerNumber { set; get; }
    }
}