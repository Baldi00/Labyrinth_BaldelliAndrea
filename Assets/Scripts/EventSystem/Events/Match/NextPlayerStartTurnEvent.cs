namespace DBGA.EventSystem
{
    public class NextPlayerStartTurnEvent : GameEvent
    {
        public int nextPlayerNumber;
        public int currentPlayerArrows;
    }
}