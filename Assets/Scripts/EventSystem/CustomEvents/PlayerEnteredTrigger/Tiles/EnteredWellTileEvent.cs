namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player enters in a tile containing a well
    /// </summary>
    public class EnteredWellTileEvent : GameEvent
    {
        public int playerNumber;
    }
}