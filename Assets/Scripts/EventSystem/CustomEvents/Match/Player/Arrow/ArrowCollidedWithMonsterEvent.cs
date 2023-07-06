namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when an arrow collides with a monster
    /// </summary>
    public class ArrowCollidedWithMonsterEvent : IGameEvent
    {
        public int PlayerNumber { set; get; }
    }
}