namespace DBGA.EventSystem
{
    /// <summary>
    /// Called when a player enters in or exits from a tile adjacent to a monster
    /// </summary>
    public class MonsterTileAdjacentEvent : GameEvent
    {
        public int playerNumber;
        public bool isPlayerInside;
    }
}