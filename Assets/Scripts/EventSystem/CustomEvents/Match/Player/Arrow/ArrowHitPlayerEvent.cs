namespace DBGA.EventSystem
{
    // Called when an arrow collides with a player
    public class ArrowHitPlayerEvent : GameEvent
    {
        public int shooterPlayerNumber;
        public int hitPlayerNumber;
    }
}