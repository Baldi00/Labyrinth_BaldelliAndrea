namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player enters in or exits from a tile adjacent to a map element
    /// </summary>
	public abstract class TileAdjacentMapElementEvent : IGameEvent
	{
        public int PlayerNumber { set; get; }
        public bool IsPlayerInside { set; get; }
    }
}
