namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player enters in a tile containing a map element
    /// </summary>
    public abstract class EnteredMapElementTileEvent : IGameEvent
    {
        public int PlayerNumber { set; get; }
    }
}
