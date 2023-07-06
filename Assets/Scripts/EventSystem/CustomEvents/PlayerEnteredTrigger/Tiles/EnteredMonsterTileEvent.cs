namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player enters in a tile containing a monster
    /// </summary>
    public class EnteredMonsterTileEvent : GameEvent
    {
        public int playerNumber;
    }
}